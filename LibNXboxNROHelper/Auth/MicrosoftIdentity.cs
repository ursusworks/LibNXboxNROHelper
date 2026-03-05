using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ursus.Xbox
{
    public static class MsalTokenProvider
    {
        private static readonly string CacheDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UrsusWorks");
        private static readonly string CachePath = Path.Combine(CacheDir, "msal_cache.bin3");
        private static readonly object CacheLock = new();

        private static IPublicClientApplication CreateApp(string clientId, string authorityTenant)
        {
            Directory.CreateDirectory(CacheDir);

            var builder = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, authorityTenant);

            // Set platform-specific redirect URI
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")))
            {
                builder = builder.WithRedirectUri($"msal{clientId}://auth");
            }
            else
            {
                builder = builder.WithDefaultRedirectUri();
            }

            var app = builder.Build();

            // Only set custom token cache on non-mobile platforms
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")) &&
                !RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS")) &&
                !RuntimeInformation.IsOSPlatform(OSPlatform.Create("MACCATALYST")))
            {
                app.UserTokenCache.SetBeforeAccess(args =>
                {
                    lock (CacheLock)
                    {
                        if (File.Exists(CachePath))
                        {
                            var data = File.ReadAllBytes(CachePath);
                            args.TokenCache.DeserializeMsalV3(Unprotect(data), shouldClearExistingCache: true);
                        }
                    }
                });

                app.UserTokenCache.SetAfterAccess(args =>
                {
                    if (!args.HasStateChanged) return;
                    lock (CacheLock)
                    {
                        var data = args.TokenCache.SerializeMsalV3();
                        File.WriteAllBytes(CachePath, Protect(data));
                    }
                });
            }

            return app;
        }

        private static byte[] Protect(byte[] data)
        {
            // Use DPAPI only on Windows, plain on Android/iOS/macOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return data;
        }

        private static byte[] Unprotect(byte[] data)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return data;
        }

        // Ensure a fresh access token (silent refresh if near expiry). Falls back to Device Code.
        public static async Task<AuthenticationResult> AcquireOrRefreshAsync(
            string clientId,
            string authorityTenant = "consumers",
            string[]? scopes = null,
            TimeSpan? minValidity = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("clientId is required.", nameof(clientId));

            scopes ??= new[] { "XboxLive.signin", "offline_access", "profile", "openid" };
            minValidity ??= TimeSpan.FromMinutes(5);

            var app = CreateApp(clientId, authorityTenant);

            var account = (await app.GetAccountsAsync().ConfigureAwait(false)).FirstOrDefault();

            try
            {
                var result = await app.AcquireTokenSilent(scopes, account)
                                      .ExecuteAsync(ct).ConfigureAwait(false);

                // Refresh proactively if expiring soon
                if (result.ExpiresOn - DateTimeOffset.UtcNow <= minValidity.Value)
                {
                    result = await app.AcquireTokenSilent(scopes, result.Account)
                                      .WithForceRefresh(true)
                                      .ExecuteAsync(ct).ConfigureAwait(false);
                }

                return result;
            }
            catch (MsalUiRequiredException)
            {
                var result = await app.AcquireTokenWithDeviceCode(scopes, callback =>
                {
                    Console.WriteLine(callback.Message);
                    return Task.CompletedTask;
                }).ExecuteAsync(ct).ConfigureAwait(false);

                return result;
            }
        }

        // Legacy helper (kept for compatibility)
        public static Task<string> AcquireWithDeviceCodeAsync(
            string clientId,
            string authorityTenant = "consumers",
            string[]? scopes = null,
            CancellationToken ct = default)
            => AcquireOrRefreshAsync(clientId, authorityTenant, scopes, ct: ct)
                .ContinueWith(t => t.Result.AccessToken, ct);
    }
}
