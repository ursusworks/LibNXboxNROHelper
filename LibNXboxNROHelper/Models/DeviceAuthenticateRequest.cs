using System.Text.Json.Serialization;

namespace Ursus.Xbox.Models
{
    public sealed class DeviceAuthenticateRequest
    {
        [JsonPropertyName("Properties")]
        public DeviceAuthProperties Properties { get; set; } = new();

        [JsonPropertyName("RelyingParty")]
        public string RelyingParty { get; set; } = "http://auth.xboxlive.com";

        [JsonPropertyName("TokenType")]
        public string TokenType { get; set; } = "JWT";
    }

    public sealed class DeviceAuthProperties
    {
        [JsonPropertyName("AuthMethod")]
        public string AuthMethod { get; set; } = "ProofOfPossession";

        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("DeviceType")]
        public string DeviceType { get; set; } = string.Empty;

        [JsonPropertyName("SerialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        [JsonPropertyName("Version")]
        public string Version { get; set; } = string.Empty;

        // JWK for the public key
        [JsonPropertyName("ProofKey")]
        public ProofKey ProofKey { get; set; } = default!;
    }

    public sealed class DeviceAuthenticateResponse
    {
        [JsonPropertyName("IssueInstant")]
        public DateTimeOffset IssueInstant { get; set; }

        [JsonPropertyName("NotAfter")]
        public DateTimeOffset NotAfter { get; set; }

        [JsonPropertyName("Token")]
        public string Token { get; set; } = string.Empty;
    }

    public class DisplayClaims
    {
        [JsonPropertyName("xdi")]
        public XdiClaim[] Xdi { get; set; } = Array.Empty<XdiClaim>();
    }

    public class XdiClaim
    {
        [JsonPropertyName("did")]
        public string Did { get; set; } = string.Empty;
        [JsonPropertyName("dcs")]
        public string Dcs { get; set; } = string.Empty;
    }

    public class ProofKey
    {
        [JsonPropertyName("alg")]
        public string Alg { get; set; } = "ES256";

        [JsonPropertyName("crv")]
        public string Crv { get; set; } = "P-256";

        [JsonPropertyName("kty")]
        public string Kty { get; set; } = "EC";

        [JsonPropertyName("use")]
        public string Use { get; set; } = "sig";

        [JsonPropertyName("x")]
        public string X { get; set; }

        [JsonPropertyName("y")]
        public string Y { get; set; }
    }
}