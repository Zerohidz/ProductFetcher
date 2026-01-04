namespace ProductFetcher.Models;

/// <summary>
/// Price information for a product
/// </summary>
public record PriceDetails
{
    public required decimal DiscountedPrice { get; init; }
    public required decimal OriginalPrice { get; init; }
    public required string CurrencyCode { get; init; }
}
