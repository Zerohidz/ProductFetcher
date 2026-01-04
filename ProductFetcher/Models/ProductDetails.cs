using System.Text.Json.Serialization;

namespace ProductFetcher.Models;

/// <summary>
/// Detailed product information
/// Example: { "attributes": [{"key": "Color", "value": "Red"}], "description": "..." }
/// </summary>
public record ProductDetails
{
    /// <summary>
    /// Product attributes as key-value pairs
    /// Python: list[dict[str, str]]
    /// </summary>
    [JsonPropertyName("attributes")]
    public required List<Dictionary<string, string>> Attributes { get; init; }
    
    [JsonPropertyName("description")]
    public required string Description { get; init; }
}
