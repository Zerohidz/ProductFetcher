namespace ProductFetcher.Models;

/// <summary>
/// Product attribute (key-value pair)
/// </summary>
public record ProductAttribute
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}

/// <summary>
/// Detailed product information
/// </summary>
public record ProductDetails
{
    public required List<ProductAttribute> Attributes { get; init; }
    public required string Description { get; init; }
}
