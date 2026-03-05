using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ursus.Xbox;
using Ursus.Xbox.Helpers;
using Ursus.Xbox.Models;

namespace Ursus.xCloudNROHelper.Auth
{
    public sealed class XboxAuthClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        private readonly XboxRequestSigner _signer;

        public XboxAuthClient(HttpClient http)
        {
            _http = http;
            _signer = new XboxRequestSigner();
        }

        public async Task<XboxUserAuthenticateResponse> GetUserTokenAsync(string msaAccessToken, CancellationToken ct = default)
        {
            var url = "https://user.auth.xboxlive.com/user/authenticate";

            var payload = new XboxUserAuthenticateRequest
            {
                Properties = new XboxUserAuthProperties
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = $"d={msaAccessToken}"
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            req.Headers.Accept.ParseAdd("application/json");

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                throw new XboxApiException("Failed to obtain Xbox User Token.", resp.StatusCode, body);

            var result = JsonSerializer.Deserialize<XboxUserAuthenticateResponse>(body, JsonOptions)
                         ?? throw new XboxApiException("Empty response deserializing Xbox User Token.", resp.StatusCode, body);
            if (string.IsNullOrWhiteSpace(result.Token))
                throw new XboxApiException("Xbox User Token missing in response.", resp.StatusCode, body);

            return result;
        }

        public async Task<XstsAuthorizeResponse> GetXstsTokenAsync(string userToken, string sandbox = "RETAIL", string relyingParty = "http://xboxlive.com", CancellationToken ct = default)
        {
            var url = "https://xsts.auth.xboxlive.com/xsts/authorize";

            var payload = new XstsAuthorizeRequest
            {
                RelyingParty = relyingParty,
                TokenType = "JWT",
                Properties = new XstsProperties
                {
                    UserTokens = new[] { userToken },
                    SandboxId = sandbox,
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            req.Headers.Accept.ParseAdd("application/json");
            req.Headers.TryAddWithoutValidation("x-xbl-contract-version", "1");

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new XboxApiException("Failed to obtain XSTS token.", resp.StatusCode, body);

            var result = JsonSerializer.Deserialize<XstsAuthorizeResponse>(body, JsonOptions)
                         ?? throw new XboxApiException("Empty response deserializing XSTS token.", resp.StatusCode, body);
            if (string.IsNullOrWhiteSpace(result.Token))
                throw new XboxApiException("XSTS token missing in response.", resp.StatusCode, body);

            return result;
        }

        public async Task<DeviceAuthenticateResponse> GetDeviceTokenAsync()
        {
            var deviceId = Guid.NewGuid().ToString().ToUpperInvariant();

            var JwkKey = JsonSerializer.Serialize(_signer.GetProofKey());

            var requestBody = new DeviceAuthenticateRequest
            {
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT",
                Properties = new DeviceAuthProperties
                {
                    AuthMethod = "ProofOfPossession",
                    Id = GenerateDeviceId("ANDROID"),
                    DeviceType = "Android",
                    Version = "8.0.0",
                    ProofKey = _signer.GetProofKey()
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            });


            var request = new HttpRequestMessage(HttpMethod.Post, "https://device.auth.xboxlive.com/device/authenticate");

            request.Headers.Add("x-xbl-contract-version", "1");
            request.Headers.Add("MS-CV", GenerateCorrelationVector());
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            await _signer.SignRequestAsync(request, signingPolicyVersion: 1, maxBodyBytes: 8192);
            HttpResponseMessage response = await _http.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed:  {response.StatusCode}\n{responseBody}");
            }

            var result = JsonSerializer.Deserialize<DeviceAuthenticateResponse>(responseBody);

            return result;
        }

        public async Task<string> GetTitleTokenAsync(string msalToken, string deviceToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://title.auth.xboxlive.com/title/authenticate");

            request.Headers.Add("x-xbl-contract-version", "1");
            request.Headers.Add("MS-CV", GenerateCorrelationVector());

            var body = new
            {
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT",
                Properties = new
                {
                    AuthMethod = "RPS",
                    DeviceToken = deviceToken,
                    RpsTicket = $"t={msalToken}",
                    SiteName = "user.auth.xboxlive.com"
                }
            };

            var jsonBody = JsonSerializer.Serialize(body);

            Console.WriteLine(jsonBody);

            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _http.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) 
            {
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(responseBody);
            }

            return responseBody;
        }

        public async Task<XstsAuthorizeResponse> GetGssvTokenAsync(string userXstsToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://xsts.auth.xboxlive.com/xsts/authorize");

            request.Headers.Add("x-xbl-contract-version", "1");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Origin", "https://www.xbox.com");
            request.Headers.Add("Referer", "https://www.xbox.com/");
            request.Headers.Add("ms-cv", "0");

            var body = new
            {
                Properties = new
                {
                    SandboxId = "RETAIL",
                    UserTokens = new[] { userXstsToken }
                },
                RelyingParty = "http://gssv.xboxlive.com/",
                TokenType = "JWT"
            };

            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode + " " + content);
            }

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<XstsAuthorizeResponse>(content);
        }
        private async Task<HttpResponseMessage> SendSignedRequestAsync(HttpMethod method, string url, object body = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("x-xbl-contract-version", "1");

            if (body != null)
            {
                string json = JsonSerializer.Serialize(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            // Sign the request
            await _signer.SignRequestAsync(request);

            // Send the request
            return await _http.SendAsync(request);
        }

        private static string GenerateDeviceId(string deviceType = "Win32")
        {
            Guid deviceId = Guid.NewGuid();

            return deviceType.ToUpperInvariant() switch
            {
                "ANDROID" or "NINTENDO" => $"{{{deviceId}}}", // {guid}
                "IOS" => deviceId.ToString().ToUpperInvariant(), // GUID in uppercase
                _ => deviceId.ToString() // Standard lowercase guid
            };
        }

        private string GenerateCorrelationVector()
        {
            // Format: base64guid. 0
            var guid = Guid.NewGuid().ToByteArray();
            var base64 = Convert.ToBase64String(guid)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return $"{base64}.0";
        }

    }
}
