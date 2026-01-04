using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProductFetcher.Models;
using ProductFetcher.Utils;

namespace ProductFetcher.Services;

/// <summary>
/// Service for fetching detailed product information
/// Equivalent to Python's product_details_fetcher.py
/// </summary>
public class ProductDetailsService
{
    private const string HtmlContentApiUrl = "https://apigw.trendyol.com/discovery-web-productgw-service/api/product-detail/{0}/html-content";

    /// <summary>
    /// Gets product details (attributes and descriptions) from product URL
    /// </summary>
    public async Task<ProductDetails> GetProductDetailsAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await HttpClientHelper.GetWithRandomUserAgentAsync(url, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        // Extract JSON from HTML
        var productJsonText = HtmlParser.ExtractProductJsonFromHtml(html, url);
        var productDetailsJson = JsonSerializer.Deserialize<HtmlParser.ProductDetailsParser.ProductDetailsJson>(productJsonText);

        var product = productDetailsJson?.Product ?? throw new InvalidOperationException("Product data not found in JSON");
        var attributeList = product.Attributes ?? [];
        var descriptionList = product.Descriptions ?? [];

        // Process descriptions into bullet list string
        var description = ProcessDescriptions(descriptionList);

        // Process attributes into list[dict]
        var attributes = ProcessAttributes(attributeList);

        return new ProductDetails
        {
            Attributes = attributes,
            Description = description
        };
    }

    /// <summary>
    /// Gets product description HTML content from API
    /// </summary>
    public async Task<string> GetProductDescriptionAsync(int productId, CancellationToken cancellationToken = default)
    {
        var url = string.Format(HtmlContentApiUrl, productId);

        try
        {
            var apiResponse = await FetchApiDataAsync(url, cancellationToken);
            var htmlContent = apiResponse?.Result?.Content ?? string.Empty;

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return string.Empty;
            }

            // Extract text from HTML
            var textContent = await HtmlParser.ExtractTextFromHtmlAsync(htmlContent);
            return textContent;
        }
        catch (HttpRequestException)
        {
            return string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"Ürün açıklaması alınırken hata oluştu: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Fetches and validates API data
    /// </summary>
    private async Task<HtmlContentApiResponse?> FetchApiDataAsync(string url, CancellationToken cancellationToken)
    {
        var response = await HttpClientHelper.GetWithRandomUserAgentAsync(url, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<HtmlContentApiResponse>(cancellationToken);

        if (data == null || !data.IsSuccess || data.StatusCode != 200)
        {
            throw new InvalidOperationException($"API hatası: {data?.Error}");
        }

        return data;
    }

    /// <summary>
    /// Process descriptions into bullet list string
    /// Matches Python logic: filters out items before "gün içinde ücretsiz iade"
    /// </summary>
    private static string ProcessDescriptions(List<HtmlParser.ProductDetailsParser.DescriptionJson> descriptionList)
    {
        if (descriptionList.Count == 0)
        {
            return string.Empty;
        }

        // Check if first item is a dict with "text" property
        if (descriptionList[0].Text == null)
        {
            return descriptionList.ToString() ?? string.Empty;
        }

        // Find the "gün içinde ücretsiz iade" item
        var freeReturnIndex = -1;
        for (int i = 0; i < descriptionList.Count; i++)
        {
            if (descriptionList[i].Text?.Contains("gün içinde ücretsiz iade. Detaylı bilgi") == true)
            {
                freeReturnIndex = i;
                break;
            }
        }

        // Take items after the free return item (or all if not found)
        var relevantDescriptions = freeReturnIndex >= 0
            ? descriptionList.Skip(freeReturnIndex + 1)
            : descriptionList;

        var bulletPoints = relevantDescriptions
            .Where(d => !string.IsNullOrWhiteSpace(d.Text))
            .Select(d => $"- {d.Text}");

        return string.Join("\n", bulletPoints);
    }

    /// <summary>
    /// Process attributes into list of dictionaries
    /// </summary>
    private static List<Dictionary<string, string>> ProcessAttributes(List<HtmlParser.ProductDetailsParser.AttributeJson> attributeList)
    {
        var attributes = new List<Dictionary<string, string>>();

        foreach (var attr in attributeList)
        {
            var key = attr.Key?.Name?.Trim();
            var value = attr.Value?.Name?.Trim();

            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {
                attributes.Add(new Dictionary<string, string>
                {
                    ["key"] = key,
                    ["value"] = value
                });
            }
        }

        return attributes;
    }

    /// <summary>
    /// API response structure for HTML content endpoint
    /// </summary>
    private class HtmlContentApiResponse
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("result")]
        public HtmlContentResult? Result { get; set; }
    }

    private class HtmlContentResult
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
