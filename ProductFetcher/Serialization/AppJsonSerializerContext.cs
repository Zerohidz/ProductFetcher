using System.Text.Json.Serialization;
using ProductFetcher.Models;
using ProductFetcher.Services;
using ProductFetcher.Utils;

namespace ProductFetcher.Serialization;

[JsonSerializable(typeof(List<Product>))]
[JsonSerializable(typeof(ProductFetcherService.ApiResponse))]
[JsonSerializable(typeof(ProductDetailsService.HtmlContentApiResponse))]
[JsonSerializable(typeof(HtmlParser.ProductDetailsParser.ProductJson), TypeInfoPropertyName = "ParserProductJson")]
[JsonSerializable(typeof(List<Program.FailedProduct>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
