using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ursus.Xbox.Models;

namespace Ursus.Xbox
{
    public sealed class XboxApiClient
    {
        private readonly HttpClient _http;
        private readonly string _authorizationValue;
        private readonly string _locale;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // uhs: from XSTS DisplayClaims
        // xstsToken: XSTS JWT token
        public XboxApiClient(HttpClient http, string uhs, string xstsToken, string? locale = null)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            if (string.IsNullOrWhiteSpace(uhs)) throw new ArgumentException("UHS is required.", nameof(uhs));
            if (string.IsNullOrWhiteSpace(xstsToken)) throw new ArgumentException("XSTS token is required.", nameof(xstsToken));

            _authorizationValue = $"XBL3.0 x={uhs};{xstsToken}";
            _locale = "en-US";
        }



        public async Task<ProfileSettingsResponse> GetMyProfileAsync(CancellationToken ct = default)
        {
            var endpoint = "https://profile.xboxlive.com/users/me/profile/settings?settings=AppDisplayName,Gamerscore,Gamertag";
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ConfigureCommonHeaders(req, contractVersion: 3);



            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                throw new XboxApiException("Failed to fetch profile settings.", resp.StatusCode, body);

            var result = JsonSerializer.Deserialize<ProfileSettingsResponse>(body, JsonOptions)
                         ?? throw new XboxApiException("Empty profile response.", resp.StatusCode, body);
            return result;
        }

        public async Task<string> GetJsonAsync(string url, int contractVersion, IDictionary<string, string?>? query, CancellationToken ct = default)
        {
            var finalUrl = AppendQuery(url, query);

            using var req = new HttpRequestMessage(HttpMethod.Get, finalUrl);
            ConfigureCommonHeaders(req, contractVersion);

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                throw new XboxApiException($"GET {finalUrl} failed.", resp.StatusCode, body);
            return body;
        }

        public async Task<string> PostJsonAsync<T>(string url, int contractVersion, T payload, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            ConfigureCommonHeaders(req, contractVersion);

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                throw new XboxApiException($"POST {url} failed.", resp.StatusCode, body);
            return body;
        }

        public async Task<string> SendXccsCommandAsync(object payload, CancellationToken ct = default)
        {
            var url = $"https://xccs.xboxlive.com/commands";
            var json = JsonSerializer.Serialize(payload);

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            Console.WriteLine(json);

            // Contract 2 + companion headers
            ConfigureCommonHeaders(req, contractVersion: 2);
            req.Headers.TryAddWithoutValidation("x-xbl-client-type", "Companion");
            req.Headers.TryAddWithoutValidation("x-xbl-client-version", "1.0.0");
            req.Headers.TryAddWithoutValidation("skillplatform", "RemoteManagement");

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                throw new XboxApiException($"POST {url} failed.", resp.StatusCode, body);
            return body;
        }

        public async Task<string> GetXccsCommandStatusAsync(string sessionId, string deviceId, CancellationToken ct = default)
        {
            var url = $"https://xccs.xboxlive.com/opStatus";


            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            ConfigureCommonHeaders(req, contractVersion: 3);
            req.Headers.TryAddWithoutValidation("x-xbl-client-type", "Companion");
            req.Headers.TryAddWithoutValidation("x-xbl-client-version", "1.0.0");
            req.Headers.TryAddWithoutValidation("skillplatform", "RemoteManagement");
            req.Headers.TryAddWithoutValidation("x-xbl-deviceId", deviceId);
            req.Headers.TryAddWithoutValidation("x-xbl-opId", sessionId);

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                //_logger.LogError("XCCS command failed: {Status} - {Body}", resp.StatusCode, body);
                throw new XboxApiException($"POST {url} failed.", resp.StatusCode, body);
            }

            return body;
        }

        public Task<string> GetAchievementsForTitleAsync(string xuid, uint titleId, int maxItems = 200, int skipItems = 0, CancellationToken ct = default)
            => GetJsonAsync($"https://achievements.xboxlive.com/users/xuid({xuid})/achievements?titleId={titleId}&maxItems={maxItems}&skipItems={skipItems}", contractVersion: 3, query: null, ct);

        public Task<string> GetTitleHistoryAsync(string xuid, CancellationToken ct = default)
            => GetJsonAsync($"https://titlehub.xboxlive.com/users/xuid({xuid})/titles/titlehistory/decoration/detail,achievement,scid,stats,contentBoard", contractVersion: 2, query: null, ct);

        public Task<string> GetTitleByID(string xuid, string titleId, CancellationToken ct = default)
            => GetJsonAsync($"https://titlehub.xboxlive.com/users/xuid({xuid})/titles/titleid({titleId})/decoration/detail,alternateTitleId,scid,achievement", contractVersion: 2, query: null, ct);

        public Task<string> GetConsolesAsync(CancellationToken ct = default)
            => GetJsonAsync($"https://xccs.xboxlive.com/lists/devices", contractVersion: 2,
                query: new Dictionary<string, string?> { { "includeStorageDevices", "true" } },
                ct);

        public Task<string> GetConsolesStatusAsync(string consoleId, CancellationToken ct = default)
            => GetJsonAsync($"https://xccs.xboxlive.com/consoles/{consoleId}", contractVersion: 2,
                query: null,
                ct);

        public Task<string> ActivateTitleAsync(string consoleId, string productId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "Shell",
                command = "ActivateApplicationWithOneStoreProductId",
                sessionId = Guid.NewGuid(),
                sourceId = "com.microsoft.smartglass",
                device_id = consoleId,
                parameters = new[] {
                    new { OneStoreProductID = productId }
                }

            }, ct);

        public Task<string> PowerOnAsync(string consoleId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "Power",
                command = "WakeUp",
                sessionId = Guid.NewGuid(),
                sourceId = "com.microsoft.smartglass",
                device_id = consoleId

            }, ct);

        public Task<string> PowerOffAsync(string consoleId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "Power",
                command = "TurnOff",
                sessionId = Guid.NewGuid(),
                sourceId = "com.microsoft.smartglass",
                device_id = consoleId
            }, ct);

        public Task<string> RebootAsync(string consoleId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "Power",
                command = "Reboot",
                sessionId = Guid.NewGuid(),
                sourceId = "com.microsoft.smartglass",
                device_id = consoleId
            }, ct);

        public Task<string> RemoteAccessConfigAsync(string consoleId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "Config",
                command = "RemoteAccess",
                sessionId = Guid.NewGuid(),
                sourceId = "com.microsoft.smartglass",
                device_id = consoleId
            }, ct);

        public Task<string> AllowConsoleStreamingAsync(string consoleId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "Config",
                command = "AllowConsoleStreaming",
                device_id = consoleId,
                enabled = true
            }, ct);

        public Task<string> StartStreamingServiceAsync(string consoleId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "GameStreaming",
                command = "StartStreamingManagementService",
                sessionId = Guid.NewGuid(),
                sourceId = "com.microsoft.smartglass",
                device_id = consoleId
            }, ct);

        public Task<string> CaptureScreenShotAsync(string consoleId, CancellationToken ct = default)
            => SendXccsCommandAsync(new
            {
                destination = "Xbox",
                type = "Game",
                command = "CaptureScreenshot",
                sessionId = Guid.NewGuid(),
                sourceId = "com.microsoft.smartglass",
                device_id = consoleId
            }, ct);

        public Task<string> GetUserPresenceAsync(CancellationToken ct = default)
            => GetJsonAsync($"https://userpresence.xboxlive.com/users/me?level=all", contractVersion: 2, query: null, ct);

        public Task<string> GetUserProfile(CancellationToken ct = default)
            => PostJsonAsync($"https://profile.xboxlive.com/users/batch/profile/settings", 
                contractVersion: 2,
                new
                {
                    userIds = new[] { "me" },
                    settings = new[]
                    {
                        "AppDisplayName",
                        "GameDisplayName",
                        "Gamerscore",
                        "Gamertag",
                        "AccountTier",
                        "XboxOneRep",
                        "PreferredColor",
                        "Bio",
                        "Location",
                        "RealName",
                        "TenureLevel",
                        "WebDisplayName"
                    }
                },
                ct);

        private void ConfigureCommonHeaders(HttpRequestMessage req, int contractVersion)
        {
            req.Headers.Accept.ParseAdd("application/json");
            req.Headers.TryAddWithoutValidation("Authorization", _authorizationValue);
            req.Headers.TryAddWithoutValidation("x-xbl-contract-version", contractVersion.ToString());
            req.Headers.TryAddWithoutValidation("Accept-Language", _locale); // e.g., "en-US"
        }

        private static string AppendQuery(string url, IDictionary<string, string?>? query)
        {
            if (query is null || query.Count == 0) return url;

            var builder = new UriBuilder(url);
            var existing = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(builder.Query))
            {
                var q = builder.Query.TrimStart('?');
                foreach (var pair in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var idx = pair.IndexOf('=');
                    if (idx < 0)
                    {
                        existing[Uri.UnescapeDataString(pair)] = null;
                    }
                    else
                    {
                        var key = Uri.UnescapeDataString(pair[..idx]);
                        var val = Uri.UnescapeDataString(pair[(idx + 1)..]);
                        existing[key] = val;
                    }
                }
            }

            foreach (var kv in query)
                existing[kv.Key] = kv.Value;

            var sb = new StringBuilder();
            foreach (var kv in existing)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(Uri.EscapeDataString(kv.Key));
                if (kv.Value is not null)
                {
                    sb.Append('=');
                    sb.Append(Uri.EscapeDataString(kv.Value));
                }
            }

            builder.Query = sb.ToString();
            return builder.Uri.ToString();
        }
    }


    public sealed class XboxApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseBody { get; }

        public XboxApiException(string message, HttpStatusCode statusCode, string? responseBody = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
