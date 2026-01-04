using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProductFetcher.Utils;

/// <summary>
/// HTTP client helper with user agent rotation
/// Equivalent to Python's header_utils.py
/// </summary>
public static class HttpClientHelper
{
    private static readonly string[] UserAgents = 
    [
        // Windows 10 - Chrome
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
        
        // Windows 11 - Chrome
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Chrome/124.0.0.0 Safari/537.36",
        
        // Windows 10 - Firefox
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0",
        
        // Windows 11 - Firefox
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:124.0) Gecko/20100101 Firefox/124.0",
        
        // Windows 10 - Microsoft Edge
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.2478.67",
        
        // Windows 11 - Microsoft Edge
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.2478.67",
    ];

    private static readonly Random Random = new();
    private static readonly HttpClient SharedClient = CreateHttpClient();

    /// <summary>
    /// Gets a random user agent from the list
    /// </summary>
    public static string GetRandomUserAgent()
    {
        return UserAgents[Random.Next(UserAgents.Length)];
    }

    /// <summary>
    /// Creates a new HttpClient with default configuration
    /// </summary>
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        });
        
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        return client;
    }

    /// <summary>
    /// Gets the shared HttpClient instance
    /// Use this for all HTTP requests to benefit from connection pooling
    /// </summary>
    public static HttpClient GetClient()
    {
        return SharedClient;
    }

    /// <summary>
    /// Performs a GET request with a random user agent
    /// </summary>
    public static async Task<HttpResponseMessage> GetWithRandomUserAgentAsync(string url, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
    {
        var uriBuilder = new UriBuilder(url);
        
        if (queryParams?.Count > 0)
        {
            var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            uriBuilder.Query = query;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
        request.Headers.TryAddWithoutValidation("User-Agent", GetRandomUserAgent());

        return await SharedClient.SendAsync(request, cancellationToken);
    }

}
