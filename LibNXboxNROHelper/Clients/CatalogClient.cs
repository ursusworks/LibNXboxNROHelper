using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ursus.Xbox.Models;


namespace Ursus.Xbox.Gamepass
{
    /// <summary>
    /// Client for interacting with the Xbox Game Pass Catalog API
    /// </summary>
    public class GamePassCatalogClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://catalog.gamepass.com";
        private readonly string _appName;
        private readonly string _appVersion;
        private bool _disposed;

        public GamePassCatalogClient(string appName = "GamePassCatalogClient", string appVersion = "1.0.0", HttpClient httpClient = null)
        {
            _appName = appName ?? throw new ArgumentNullException(nameof(appName));
            _appVersion = appVersion ?? throw new ArgumentNullException(nameof(appVersion));
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }
 
        public async Task<CatalogResponse> GetProductsAsync(
            List<string> productIds,
            string token,
            string market = "US",
            string language = "en-us",
            string hydration = "RemoteHighSapphire0",
            CancellationToken cancellationToken = default)
            {
            if (productIds == null || productIds.Count == 0)
                throw new ArgumentException("Product IDs cannot be null or empty", nameof(productIds));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            var path = $"/v3/products?hydration={hydration}&market={market}&language={language}";

            var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("ms-cv", "0.0");
            request.Headers.Add("calling-app-name", _appName);
            request.Headers.Add("calling-app-version", _appVersion);

            var requestBody = new CatalogRequest
            {
                Products = productIds
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            //Console.WriteLine(responseContent);
            
            var catalogResponse = JsonSerializer.Deserialize<CatalogResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            

            return catalogResponse;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}