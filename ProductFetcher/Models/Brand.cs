namespace ProductFetcher.Models;

/// <summary>
/// Represents a brand/manufacturer
/// </summary>
public record Brand
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
