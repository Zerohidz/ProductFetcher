using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Parser;

namespace ProductFetcher.Utils;

/// <summary>
/// HTML parsing and JSON extraction utilities
/// Equivalent to Python's product_details_fetcher.py extraction logic
/// </summary>
public static partial class HtmlParser
{
    /// <summary>
    /// Extracts JSON product state from HTML using regex
    /// Matches Python's __extract_initial_state function
    /// </summary>
    public static string ExtractProductJsonFromHtml(string html, string url)
    {
        // Extract product_id from URL (format: /brand/product-name-p-{product_id})
        // Note: URL can have multiple -p- patterns, we take the LAST one
        var productIdMatches = ProductIdRegex().Matches(url);
        
        if (productIdMatches.Count == 0)
        {
            throw new InvalidOperationException("URL'den product_id çıkarılamadı");
        }

        var productId = productIdMatches[^1].Groups[1].Value; // Take last match

        // Find the JSON object starting with "product":{"id":{product_id},
        var pattern = $"\"product\":{{\"id\":{productId},";
        var matchIndex = html.IndexOf(pattern, StringComparison.Ordinal);
        
        if (matchIndex == -1)
        {
            throw new InvalidOperationException($"Product ID {productId} için JSON bulunamadı");
        }

        // Find the start of the JSON object
        var start = matchIndex + "\"product\":".Length;

        // Use simple brace counter to find the end of JSON object
        return ExtractJsonObject(html, start);
    }

    /// <summary>
    /// Extracts a complete JSON object starting from the given position
    /// Uses brace counting and proper string escape handling
    /// </summary>
    private static string ExtractJsonObject(string html, int start)
    {
        var braceCount = 0;
        var inString = false;
        var escape = false;

        for (int i = start; i < html.Length; i++)
        {
            var c = html[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }
            }
            else
            {
                if (c == '"')
                {
                    inString = true;
                }
                else if (c == '{')
                {
                    braceCount++;
                }
                else if (c == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        return html[start..(i + 1)];
                    }
                }
            }
        }

        throw new InvalidOperationException("JSON objesi kapatılamadı");
    }

    /// <summary>
    /// Extracts plain text from HTML content
    /// Matches Python's _extract_text_from_html function
    /// </summary>
    public static async Task<string> ExtractTextFromHtmlAsync(string htmlContent)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var parser = context.GetService<IHtmlParser>();
        
        if (parser == null)
        {
            return string.Empty;
        }

        var document = await parser.ParseDocumentAsync(htmlContent);

        // Find main content wrapper
        var contentWrapper = document.QuerySelector("#rich-content-wrapper");
        if (contentWrapper == null)
        {
            return string.Empty;
        }

        var cleanText = new List<string>();

        // Heading
        var h2Tag = contentWrapper.QuerySelector("h2");
        if (h2Tag != null)
        {
            var text = h2Tag.TextContent.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                cleanText.Add(text);
            }
        }

        // Paragraphs (direct children divs without images)
        var divs = contentWrapper.Children.Where(e => e.TagName.Equals("DIV", StringComparison.OrdinalIgnoreCase));
        foreach (var div in divs)
        {
            // Skip divs containing images
            if (div.QuerySelector("img") != null)
            {
                continue;
            }

            var text = div.TextContent.Trim();
            if (!string.IsNullOrWhiteSpace(text) && text != "<br>")
            {
                cleanText.Add(text);
            }
        }

        // List items
        var olTag = contentWrapper.QuerySelector("ol");
        if (olTag != null)
        {
            var listItems = olTag.QuerySelectorAll("li");
            foreach (var li in listItems)
            {
                var text = li.TextContent.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    cleanText.Add(text);
                }
            }
        }

        // Join text
        var result = string.Join("\n", cleanText);

        // Remove excessive empty lines
        result = MultipleNewlinesRegex().Replace(result, "\n");

        return result.Trim();
    }

    /// <summary>
    /// Parses product details JSON structure
    /// </summary>
    public static class ProductDetailsParser
    {
        public class ProductDetailsJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("product")]
            public ProductJson? Product { get; set; }
        }

        public class ProductJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("attributes")]
            public List<AttributeJson>? Attributes { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("descriptions")]
            public List<DescriptionJson>? Descriptions { get; set; }
        }

        public class AttributeJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("key")]
            public NameObject? Key { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("value")]
            public NameObject? Value { get; set; }
        }

        public class NameObject
        {
            [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        public class DescriptionJson
        {
            [System.Text.Json.Serialization.JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }

    // Regex patterns (C# 11+ source generators)
    [GeneratedRegex(@"-p-(\d+)", RegexOptions.Compiled)]
    private static partial Regex ProductIdRegex();

    [GeneratedRegex(@"\n\s*\n", RegexOptions.Compiled)]
    private static partial Regex MultipleNewlinesRegex();
}
