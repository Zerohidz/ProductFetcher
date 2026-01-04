using System.Text.Json;
using OfficeOpenXml;
using ProductFetcher.Models;
using ProductFetcher.Utils;

namespace ProductFetcher.Services;

/// <summary>
/// Excel file generation service
/// Equivalent to Python's excel_utils.py
/// </summary>
public class ExcelExporterService
{
    private const string TemplatesDir = "excel_templates";
    private const string NonTemplatedSuffix = "_non_templated.xlsx";
    private const int MaxImages = 8;

    private static readonly string[] CommonHeadersBase =
    [
        "Barkod", "Model Kodu", "Marka", "Kategori", "Para Birimi", "Ürün Adı",
        "Ürün Açıklaması", "Piyasa Satış Fiyatı (KDV Dahil)",
        "Trendyol'da Satılacak Fiyat (KDV Dahil)", "Ürün Stok Adedi", "Stok Kodu",
        "KDV Oranı", "Desi",
        "Görsel 1", "Görsel 2", "Görsel 3", "Görsel 4",
        "Görsel 5", "Görsel 6", "Görsel 7", "Görsel 8",
        "Sevkiyat Süresi", "Sevkiyat Tipi"
    ];

    static ExcelExporterService()
    {
        // For EPPlus 8+, we must use the License property and explicit method for non-commercial use
        ExcelPackage.License.SetNonCommercialPersonal("Zerohidz");
    }

    /// <summary>
    /// Creates Excel files for merchant's products, organized by category
    /// </summary>
    public void CreateExcelFilesForMerchant(List<Product> products, int merchantId, string templatesBaseDir = TemplatesDir)
    {
        if (products.Count == 0)
        {
            Console.WriteLine("Excel oluşturmak için ürün bulunamadı.");
            return;
        }

        // Determine merchant name and setup output directory
        var merchantName = GetMerchantName(products);
        var outputDirName = $"outputs/{merchantName}_{merchantId}";

        FileHelper.SetupClearDirectory(outputDirName);
        Console.WriteLine($"Excel dosyaları '{outputDirName}' klasörüne kaydedilecek.");

        // Group products by category
        var categories = products.GroupBy(p => p.CategoryName).ToDictionary(g => g.Key, g => g.ToList());

        // Process each category
        foreach (var (categoryName, categoryProducts) in categories)
        {
            if (categoryProducts.Count == 0)
            {
                continue;
            }

            // Load template or use default headers
            var (finalHeaders, isTemplated, excelFilenameSuffix, templatePath) = 
                LoadTemplateHeaders(categoryName, templatesBaseDir);

            // For non-templated files, extend headers with all attribute keys
            if (!isTemplated)
            {
                var attrKeys = CollectAttributeKeys(categoryProducts);
                finalHeaders = ExtendHeadersWithAttributes(finalHeaders, attrKeys);
            }

            var excelFilePath = Path.Combine(outputDirName, $"{categoryName}{excelFilenameSuffix}");

            // Build product data rows
            var productsDataForExcel = new List<Dictionary<string, object?>>();
            foreach (var product in categoryProducts)
            {
                var productAttributes = GetProductSpecificAttributes(product);
                var productRow = BuildProductRow(product, finalHeaders, productAttributes);
                productsDataForExcel.Add(productRow);
            }

            if (productsDataForExcel.Count == 0)
            {
                Console.WriteLine($"'{categoryName}' kategorisi için yazılacak veri bulunamadı.");
                continue;
            }

            // Save to Excel
            SaveDataToExcel(productsDataForExcel, finalHeaders, excelFilePath, categoryName, 
                isTemplated, categoryProducts.Count, templatePath);
        }
    }

    /// <summary>
    /// Estimates merchant name from products based on most common brand
    /// </summary>
    private static string GetMerchantName(List<Product> products)
    {
        if (products.Count == 0)
        {
            return "UnknownMerchant";
        }

        var sampleSize = Math.Min(10, products.Count);
        var randomProducts = products.OrderBy(_ => Random.Shared.Next()).Take(sampleSize).ToList();

        var brandCounts = randomProducts
            .Where(p => p.Brand != null)
            .GroupBy(p => p.Brand.Name)
            .Select(g => new { Brand = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        return brandCounts.FirstOrDefault()?.Brand ?? "UnknownMerchant";
    }

    /// <summary>
    /// Extracts product attributes into a dictionary
    /// </summary>
    private static Dictionary<string, string> GetProductSpecificAttributes(Product product)
    {
        var attributes = new Dictionary<string, string>();

        if (product.Details?.Attributes != null)
        {
            foreach (var attr in product.Details.Attributes)
            {
                if (attr.TryGetValue("key", out var key) && attr.TryGetValue("value", out var value))
                {
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        attributes[key] = value ?? string.Empty;
                    }
                }
            }
        }

        return attributes;
    }

    /// <summary>
    /// Attempts to load headers from a template file
    /// Returns: (headers, isTemplated, fileSuffix, templatePath)
    /// </summary>
    private static (List<string> headers, bool isTemplated, string fileSuffix, string? templatePath) 
        LoadTemplateHeaders(string categoryName, string templatesBaseDir)
    {
        var templatePath = Path.Combine(templatesBaseDir, $"{categoryName}.xlsx");

        if (File.Exists(templatePath))
        {
            try
            {
                using var package = new ExcelPackage(new FileInfo(templatePath));
                var worksheet = package.Workbook.Worksheets[0];

                var headers = new List<string>();
                var colCount = worksheet.Dimension?.Columns ?? 0;

                for (int col = 1; col <= colCount; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Value?.ToString() ?? string.Empty;
                    headers.Add(headerValue);
                }

                Console.WriteLine($"Kategori '{categoryName}' için şablon bulundu: {templatePath}");
                return (headers, true, ".xlsx", templatePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"'{categoryName}' için şablon okunamadı ({templatePath}): {ex.Message}. Şablonsuz devam edilecek.");
            }
        }
        else
        {
            Console.WriteLine($"Kategori '{categoryName}' için şablon bulunamadı: {templatePath}. Şablonsuz format kullanılacak.");
        }

        return (CommonHeadersBase.ToList(), false, NonTemplatedSuffix, null);
    }

    /// <summary>
    /// Collects all unique attribute keys from products
    /// </summary>
    private static HashSet<string> CollectAttributeKeys(List<Product> products)
    {
        var allKeys = new HashSet<string>();

        foreach (var product in products)
        {
            if (product.Details?.Attributes != null)
            {
                foreach (var attr in product.Details.Attributes)
                {
                    if (attr.TryGetValue("key", out var key) && !string.IsNullOrWhiteSpace(key))
                    {
                        allKeys.Add(key);
                    }
                }
            }
        }

        return allKeys;
    }

    /// <summary>
    /// Extends headers with attribute keys if not already present
    /// </summary>
    private static List<string> ExtendHeadersWithAttributes(List<string> baseHeaders, HashSet<string> attributeKeys)
    {
        var finalHeaders = new List<string>(baseHeaders);

        foreach (var key in attributeKeys.OrderBy(k => k))
        {
            if (!finalHeaders.Contains(key))
            {
                finalHeaders.Add(key);
            }
        }

        return finalHeaders;
    }

    /// <summary>
    /// Builds a row of data for a product based on headers
    /// </summary>
    private static Dictionary<string, object?> BuildProductRow(
        Product product, 
        List<string> headers, 
        Dictionary<string, string> productAttributes)
    {
        var productRow = new Dictionary<string, object?>();

        foreach (var header in headers)
        {
            productRow[header] = header switch
            {
                "Marka" => product.Brand.Name,
                "Kategori" => product.CategoryId,
                "Para Birimi" => product.Price.CurrencyCode,
                "Ürün Adı" => product.Name,
                "Ürün Açıklaması" => TextUtils.SanitizeString(
                    (product.Details?.Description ?? "") + "\n" + (product.Description ?? "")),
                "Piyasa Satış Fiyatı (KDV Dahil)" => product.Price.OriginalPrice,
                "Trendyol'da Satılacak Fiyat (KDV Dahil)" => product.Price.DiscountedPrice,
                "Görsel 1" => product.ImageUrls.Count > 0 ? product.ImageUrls[0] : "",
                "Görsel 2" => product.ImageUrls.Count > 1 ? product.ImageUrls[1] : "",
                "Görsel 3" => product.ImageUrls.Count > 2 ? product.ImageUrls[2] : "",
                "Görsel 4" => product.ImageUrls.Count > 3 ? product.ImageUrls[3] : "",
                "Görsel 5" => product.ImageUrls.Count > 4 ? product.ImageUrls[4] : "",
                "Görsel 6" => product.ImageUrls.Count > 5 ? product.ImageUrls[5] : "",
                "Görsel 7" => product.ImageUrls.Count > 6 ? product.ImageUrls[6] : "",
                "Görsel 8" => product.ImageUrls.Count > 7 ? product.ImageUrls[7] : "",
                "KDV Oranı" => product.Tax,
                _ => productAttributes.TryGetValue(header, out var value) ? value : null
            };
        }

        return productRow;
    }

    /// <summary>
    /// Saves data to Excel file
    /// </summary>
    private static void SaveDataToExcel(
        List<Dictionary<string, object?>> data,
        List<string> headers,
        string filePath,
        string categoryName,
        bool isTemplated,
        int productCount,
        string? templatePath)
    {
        try
        {
            if (isTemplated && templatePath != null && File.Exists(templatePath))
            {
                // Copy template and fill with data
                File.Copy(templatePath, filePath, overwrite: true);

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0];

                // Get headers from template
                var templateHeaders = new List<string>();
                var colCount = worksheet.Dimension?.Columns ?? 0;
                for (int col = 1; col <= colCount; col++)
                {
                    templateHeaders.Add(worksheet.Cells[1, col].Value?.ToString() ?? "");
                }

                // Write data starting from row 2
                for (int rowIdx = 0; rowIdx < data.Count; rowIdx++)
                {
                    var rowData = data[rowIdx];
                    foreach (var header in headers)
                    {
                        var templateColIdx = templateHeaders.IndexOf(header);
                        if (templateColIdx >= 0 && rowData.ContainsKey(header))
                        {
                            worksheet.Cells[rowIdx + 2, templateColIdx + 1].Value = rowData[header];
                        }
                    }
                }

                package.Save();
            }
            else
            {
                // Create new Excel file
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Products");

                // Write headers
                for (int col = 0; col < headers.Count; col++)
                {
                    worksheet.Cells[1, col + 1].Value = headers[col];
                }

                // Write data
                for (int rowIdx = 0; rowIdx < data.Count; rowIdx++)
                {
                    var rowData = data[rowIdx];
                    for (int col = 0; col < headers.Count; col++)
                    {
                        var header = headers[col];
                        if (rowData.ContainsKey(header))
                        {
                            worksheet.Cells[rowIdx + 2, col + 1].Value = rowData[header];
                        }
                    }
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                package.SaveAs(new FileInfo(filePath));
            }

            var statusMsg = isTemplated ? "şablonlu" : "şablonsuz";
            Console.WriteLine($"'{categoryName}' kategorisi için {statusMsg} Excel dosyası oluşturuldu ({productCount} ürün): {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"'{categoryName}' kategorisi için Excel ({filePath}) yazılırken hata oluştu: {ex.Message}");
        }
    }
}
