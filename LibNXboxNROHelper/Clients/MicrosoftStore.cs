using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ursus.Xbox
{
    public class MicrosoftStoreSearchClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://displaycatalog.mp.microsoft.com";

        public MicrosoftStoreSearchClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<List<Product>> SearchProductsAsync(
            string searchQuery,
            string market = "US",
            string language = "en-US",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                throw new ArgumentException("Search query cannot be empty", nameof(searchQuery));

            // This is the correct search endpoint
            var url = $"{BaseUrl}/v7.0/productFamilies/autosuggest" +
                     $"?query={Uri.EscapeDataString(searchQuery)}" +
                     $"&market={market}" +
                     $"&languages={language}" +
                     $"&productFamilyNames=Games"; // This was missing!

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("MS-CV", "0");

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"API returned {response.StatusCode}. Response: {content}");
                }

                var result = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result.Results.SelectMany(r => r.Products).Where(r => r.Type == "Game").ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching products: {ex.Message}", ex);
            }
        }

        private string BuildUrl(string path, Dictionary<string, string> queryParams)
        {
            var query = string.Join("&",
                Array.ConvertAll(queryParams.Keys.ToArray(),
                    key => $"{key}={Uri.EscapeDataString(queryParams[key])}"));

            return $"{BaseUrl}{path}?{query}";
        }
    }


    public class SearchResponse
    {
        public Result[] Results { get; set; }
        public int TotalResultCount { get; set; }
    }

    public class Result
    {
        public string ProductFamilyName { get; set; }
        public Product[] Products { get; set; }
    }

    public class Product
    {
        public string BackgroundColor { get; set; }
        public int Height { get; set; }
        public string ImageType { get; set; }
        public int Width { get; set; }
        public object[] PlatformProperties { get; set; }
        public string Icon { get; set; }
        public string ProductId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
    }

}
