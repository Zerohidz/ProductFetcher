using System.Text.Json;
using ProductFetcher.Models;
using ProductFetcher.Services;
using ProductFetcher.Utils;

namespace ProductFetcher;

/// <summary>
/// Main program orchestrating the product fetching workflow
/// Equivalent to Python's main.py
/// </summary>
public class Program
{
    private const string OutputDir = "outputs";
    private const string TestingDir = "testing";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Product Fetcher ===\n");

        // Get merchant ID from user
        Console.Write("Mağaza ID: ");
        var merchantIdInput = Console.ReadLine();

        if (!int.TryParse(merchantIdInput, out var merchantId))
        {
            Console.WriteLine("Geçersiz mağaza ID!");
            return;
        }

        try
        {
            // Step 1: Fetch all products
            Console.WriteLine("\n--- Ürünler çekiliyor ---");
            var productFetcher = new ProductFetcherService();
            var products = await productFetcher.FetchAllProductsAsync(merchantId);

            // Save raw products JSON
            FileHelper.EnsureDirectoryExists(TestingDir);
            await SaveProductsJsonAsync(products, Path.Combine(TestingDir, "products.json"));

            // Print brief report
            Console.WriteLine($"\nToplam unique ürün: {products.Count}");
            if (products.Count > 0)
            {
                var first = products[0];
                var last = products[^1];
                Console.WriteLine($"İlk ürün: {first.Name} - {first.Price.DiscountedPrice} TL");
                Console.WriteLine($"Son ürün: {last.Name} - {last.Price.DiscountedPrice} TL");
            }

            // Step 2: Fetch details and descriptions
            Console.WriteLine("\n--- Detaylar ve açıklamalar alınıyor ---");
            await FetchDetailsAndDescriptionsAsync(products);

            // Save enriched products JÊSON
            await SaveProductsJsonAsync(products, Path.Combine(TestingDir, "product_details.json"));
            Console.WriteLine("Detaylar ve açıklamalar alındı.");

            // Step 3: Generate Excel files
            Console.WriteLine("\n--- Excel dosyaları oluşturuluyor ---");
            var excelExporter = new ExcelExporterService();
            excelExporter.CreateExcelFilesForMerchant(products, merchantId);

            Console.WriteLine("\n✅ İşlem tamamlandı!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Hata: {ex.Message}");
            Console.WriteLine($"Detaylar: {ex}");
        }
    }

    /// <summary>
    /// Fetches details and descriptions for all products with error handling
    /// </summary>
    private static async Task FetchDetailsAndDescriptionsAsync(List<Product> products)
    {
        var failedProducts = new List<FailedProduct>();
        var detailsService = new ProductDetailsService();
        var processedCount = 0;
        var progress = "";

        foreach (var product in products)
        {
            processedCount++;
            progress = $"[{processedCount}/{products.Count}]";

            try
            {
                // Get product details
                var details = await detailsService.GetProductDetailsAsync(product.Url);
                product.Details = details;

                // Get product description
                var description = await detailsService.GetProductDescriptionAsync(product.Id);
                product.Description = description;

                Console.Write($"\r{progress} İşleniyor... ");

                // Random delay (0 to 0.3 seconds)
                await Task.Delay(Random.Shared.Next(0, 300));
            }
            catch (Exception ex)
            {
                // Log failed product
                failedProducts.Add(new FailedProduct
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Url = product.Url,
                    Error = ex.Message,
                    ErrorType = ex.GetType().Name
                });

                Console.WriteLine($"\n⚠️  Ürün atlandı: {product.Name} (ID: {product.Id}) - Hata: {ex.Message}");
            }
        }

        Console.WriteLine($"\r{progress} Tamamlandı!     ");

        // Save failed products if any
        if (failedProducts.Count > 0)
        {
            var failedProductsPath = Path.Combine(TestingDir, "failed_products.json");
            await File.WriteAllTextAsync(
                failedProductsPath,
                JsonSerializer.Serialize(failedProducts, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            Console.WriteLine($"\n⚠️  {failedProducts.Count} ürün işlenemedi. Detaylar: {failedProductsPath}");
        }
    }

    /// <summary>
    /// Saves products to JSON file
    /// </summary>
    private static async Task SaveProductsJsonAsync(List<Product> products, string filePath)
    {
        var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Failed product information for error logging
    /// </summary>
    private class FailedProduct
    {
        public required int ProductId { get; init; }
        public required string ProductName { get; init; }
        public required string Url { get; init; }
        public required string Error { get; init; }
        public required string ErrorType { get; init; }
    }
}
