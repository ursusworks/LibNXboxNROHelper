using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ursus.Xbox.Models
{
    public sealed class XboxUserAuthenticateRequest
    {
        [JsonPropertyName("Properties")]
        public XboxUserAuthProperties Properties { get; set; } = new();

        [JsonPropertyName("RelyingParty")]
        public string RelyingParty { get; set; } = "http://auth.xboxlive.com";

        [JsonPropertyName("TokenType")]
        public string TokenType { get; set; } = "JWT";
    }

    public sealed class XboxUserAuthProperties
    {
        [JsonPropertyName("AuthMethod")]
        public string AuthMethod { get; set; } = "RPS";

        [JsonPropertyName("SiteName")]
        public string SiteName { get; set; } = "user.auth.xboxlive.com";

        // RpsTicket = "d=<MicrosoftAccessToken>"
        [JsonPropertyName("RpsTicket")]
        public string RpsTicket { get; set; } = string.Empty;
    }

    public sealed class XboxUserAuthenticateResponse
    {
        [JsonPropertyName("IssueInstant")]
        public DateTimeOffset IssueInstant { get; set; }

        [JsonPropertyName("NotAfter")]
        public DateTimeOffset NotAfter { get; set; }

        [JsonPropertyName("Token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("DisplayClaims")]
        public XblDisplayClaims DisplayClaims { get; set; } = new();
    }

    public sealed class XblDisplayClaims
    {
        [JsonPropertyName("xui")]
        public List<XuiEntry> Xui { get; set; } = new();
    }

    public sealed class XuiEntry
    {
        [JsonPropertyName("uhs")]
        public string Uhs { get; set; } = string.Empty;
    }
}
