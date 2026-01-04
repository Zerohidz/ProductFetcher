using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProductFetcher.Models;
using ProductFetcher.Utils;

namespace ProductFetcher.Services;

/// <summary>
/// Fetches products from Trendyol merchant pages
/// Equivalent to Python's merchant_product_fetcher.py
/// </summary>
public class ProductFetcherService
{
    private const string ApiBaseUrl = "https://apigw.trendyol.com/discovery-web-searchgw-service/v2/api/infinite-scroll/sr";
    
    /// <summary>
    /// API response structure
    /// </summary>
    private class ApiResponse
    {
        [JsonPropertyName("result")]
        public required ApiResult Result { get; init; }
    }
    
    private class ApiResult
    {
        [JsonPropertyName("products")]
        public required List<ProductJson> Products { get; init; }
    }
    
    /// <summary>
    /// Raw product JSON from API (uses camelCase)
    /// </summary>
    private class ProductJson
    {
        [JsonPropertyName("id")]
        public required int Id { get; init; }
        
        [JsonPropertyName("brand")]
        public required BrandJson Brand { get; init; }
        
        [JsonPropertyName("categoryHierarchy")]
        public required string CategoryHierarchy { get; init; }
        
        [JsonPropertyName("categoryName")]
        public required string CategoryName { get; init; }
        
        [JsonPropertyName("categoryId")]
        public required int CategoryId { get; init; }
        
        [JsonPropertyName("url")]
        public required string Url { get; init; }
        
        [JsonPropertyName("name")]
        public required string Name { get; init; }
        
        [JsonPropertyName("images")]
        public required List<string> Images { get; init; }
        
        [JsonPropertyName("price")]
        public required PriceJson Price { get; init; }
        
        [JsonPropertyName("tax")]
        public required decimal Tax { get; init; }
    }
    
    private class BrandJson
    {
        [JsonPropertyName("id")]
        public required int Id { get; init; }
        
        [JsonPropertyName("name")]
        public required string Name { get; init; }
    }
    
    private class PriceJson
    {
        [JsonPropertyName("discountedPrice")]
        public required decimal DiscountedPrice { get; init; }
        
        [JsonPropertyName("originalPrice")]
        public required decimal OriginalPrice { get; init; }
        
        [JsonPropertyName("currencyCode")]
        public required string CurrencyCode { get; init; }
    }

    /// <summary>
    /// Fetches all products for a merchant using price range strategy
    /// </summary>
    public async Task<List<Product>> FetchAllProductsAsync(int merchantId, CancellationToken cancellationToken = default)
    {
        var allProducts = new List<Product>();
        var seenIds = new HashSet<int>();
        decimal? minPrice = null;
        bool limitReached = true;

        while (limitReached)
        {
            var (products, reached, lastProduct) = await FetchProductsWithPriceRangeAsync(
                merchantId, seenIds, allProducts.Count, minPrice, cancellationToken);
            
            allProducts.AddRange(products);
            limitReached = reached;

            if (lastProduct != null && allProducts.Count > 0)
            {
                var lastPrice = lastProduct.Price.OriginalPrice;
                minPrice = FindNextMinPrice(allProducts, lastPrice);
            }
            else
            {
                break;
            }
        }

        return allProducts;
    }

    /// <summary>
    /// Fetches products with a specific price range
    /// Returns: (products, limitReached, lastProduct)
    /// </summary>
    private async Task<(List<Product> products, bool limitReached, Product? lastProduct)> FetchProductsWithPriceRangeAsync(
        int merchantId,
        HashSet<int> seenIds,
        int previouslyFetchedCount,
        decimal? minPrice,
        CancellationToken cancellationToken)
    {
        var products = new List<Product>();
        int page = 1;
        Product? lastProduct = null;

        var queryParams = new Dictionary<string, string>
        {
            ["mid"] = merchantId.ToString(),
            ["os"] = "1",
            ["culture"] = "tr-TR",
            ["userGenderId"] = "1",
            ["pId"] = "0",
            ["isLegalRequirementConfirmed"] = "false",
            ["searchStrategyType"] = "DEFAULT",
            ["productStampType"] = "TypeA",
            ["scoringAlgorithmId"] = "2",
            ["fixSlotProductAdsIncluded"] = "true",
            ["channelId"] = "1",
            ["sst"] = "PRICE_BY_ASC"
        };

        if (minPrice.HasValue)
        {
            queryParams["prc"] = $"{minPrice.Value + 0.01m}-*";
        }

        while (true)
        {
            var logHeader = $"| Fiyat aralığı: {minPrice ?? 0} - ♾️  |";
            queryParams["pi"] = page.ToString();

            try
            {
                var response = await HttpClientHelper.GetWithRandomUserAgentAsync(ApiBaseUrl, queryParams, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (IsLimitError(response, responseText))
                    {
                        Console.WriteLine($"{logHeader} Sayfa limiti hatası (page {page}). Fiyat aralığı güncellenip çekmeye devam ediliyor...");
                        return (products, true, lastProduct);
                    }
                    
                    response.EnsureSuccessStatusCode();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>(cancellationToken);
                var pageProducts = apiResponse?.Result?.Products ?? [];

                if (pageProducts.Count == 0)
                {
                    Console.WriteLine($"{logHeader} No more products after page {page}.");
                    break;
                }

                // Add new products (deduplicate by ID)
                var newCount = 0;
                foreach (var productJson in pageProducts)
                {
                    if (!seenIds.Contains(productJson.Id))
                    {
                        var product = ConvertToProduct(productJson);
                        products.Add(product);
                        seenIds.Add(productJson.Id);
                        newCount++;
                        lastProduct = product;
                    }
                }

                Console.WriteLine($"{logHeader} Fetched page {page}, total products: {previouslyFetchedCount + products.Count}");

                page++;
                
                // Random delay between requests (0.2 to 0.7 seconds)
                await Task.Delay(TimeSpan.FromMilliseconds(200 + Random.Shared.Next(500)), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"{logHeader} Error on page {page}: {ex.Message}");
                throw;
            }
        }

        return (products, false, lastProduct);
    }

    /// <summary>
    /// Checks if the response indicates page limit error
    /// </summary>
    private static bool IsLimitError(HttpResponseMessage response, string responseText)
    {
        return response.StatusCode == System.Net.HttpStatusCode.NotFound 
            && responseText.Contains("Page index cannot be higher than");
    }

    /// <summary>
    /// Finds the next minimum price for pagination strategy
    /// Searches backwards from the end of the list for a price lower than lastPrice
    /// </summary>
    private static decimal FindNextMinPrice(List<Product> allProducts, decimal lastPrice)
    {
        for (int i = allProducts.Count - 1; i >= 0; i--)
        {
            if (allProducts[i].Price.OriginalPrice < lastPrice)
            {
                return allProducts[i].Price.OriginalPrice;
            }
        }
        return lastPrice;
    }

    /// <summary>
    /// Converts API JSON format (camelCase) to our Product model
    /// </summary>
    private static Product ConvertToProduct(ProductJson json)
    {
        return new Product
        {
            Id = json.Id,
            Brand = new Brand
            {
                Id = json.Brand.Id,
                Name = json.Brand.Name
            },
            CategoryHierarchy = json.CategoryHierarchy,
            CategoryName = json.CategoryName,
            CategoryId = json.CategoryId,
            Url = "https://www.trendyol.com" + json.Url,
            Name = json.Name,
            ImageUrls = json.Images.Select(img => "https://cdn.dsmcdn.com" + img).ToList(),
            Price = new PriceDetails
            {
                DiscountedPrice = json.Price.DiscountedPrice,
                OriginalPrice = json.Price.OriginalPrice,
                CurrencyCode = json.Price.CurrencyCode
            },
            Tax = json.Tax,
            Details = null,
            Description = null
        };
    }
}
