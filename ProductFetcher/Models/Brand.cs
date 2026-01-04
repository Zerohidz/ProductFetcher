using System.Text.Json.Serialization;

namespace ProductFetcher.Models;

/// <summary>
/// Represents a brand/manufacturer
/// Example: { "id": 12345, "name": "Samsung" }
/// </summary>
public record Brand
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
