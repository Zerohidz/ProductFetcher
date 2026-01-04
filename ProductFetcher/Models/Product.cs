namespace ProductFetcher.Models;

/// <summary>
/// Main product entity
/// </summary>
public record Product
{
    public required int Id { get; init; }
    public required Brand Brand { get; init; }
    public required string CategoryHierarchy { get; init; }
    public required string CategoryName { get; init; }
    public required int CategoryId { get; init; }
    public required string Url { get; init; }
    public required string Name { get; init; }
    public required List<string> ImageUrls { get; init; }
    public required PriceDetails Price { get; init; }
    public required decimal Tax { get; init; }
    
    // Optional fields - enriched later
    public ProductDetails? Details { get; init; }
    public string? Description { get; init; }
}
