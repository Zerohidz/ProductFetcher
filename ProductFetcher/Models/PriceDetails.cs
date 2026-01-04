using System.Text.Json.Serialization;

namespace ProductFetcher.Models;

/// <summary>
/// Price information for a product
/// Example: { "discountedPrice": 84.9, "originalPrice": 84.9, "currencyCode": "TRY" }
/// </summary>
public record PriceDetails
{
    [JsonPropertyName("discounted_price")]
    public required decimal DiscountedPrice { get; init; }
    
    [JsonPropertyName("original_price")]
    public required decimal OriginalPrice { get; init; }
    
    [JsonPropertyName("currency_code")]
    public required string CurrencyCode { get; init; }
}
