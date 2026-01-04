using System.Text.Json.Serialization;

namespace ProductFetcher.Models;

/// <summary>
/// Main product entity
/// Supports both API response format (camelCase) and saved format (snake_case)
/// </summary>
public record Product
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("brand")]
    public required Brand Brand { get; init; }
    
    // API uses "categoryHierarchy", saved JSON uses "category_hierarchy"
    [JsonPropertyName("category_hierarchy")]
    public required string CategoryHierarchy { get; init; }
    
    [JsonPropertyName("category_name")]
    public required string CategoryName { get; init; }
    
    [JsonPropertyName("category_id")]
    public required int CategoryId { get; init; }
    
    [JsonPropertyName("url")]
    public required string Url { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("image_urls")]
    public required List<string> ImageUrls { get; init; }
    
    [JsonPropertyName("price")]
    public required PriceDetails Price { get; init; }
    
    [JsonPropertyName("tax")]
    public required decimal Tax { get; init; }
    
    // Optional fields - enriched later
    [JsonPropertyName("details")]
    public ProductDetails? Details { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
