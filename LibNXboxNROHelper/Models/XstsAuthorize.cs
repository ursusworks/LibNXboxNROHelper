using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ursus.Xbox.Models
{
    public sealed class XstsAuthorizeRequest
    {
        [JsonPropertyName("Properties")]
        public XstsProperties Properties { get; set; } = new();

        [JsonPropertyName("RelyingParty")]
        public string RelyingParty { get; set; } = "http://xboxlive.com";

        [JsonPropertyName("TokenType")]
        public string TokenType { get; set; } = "JWT";
    }

    public sealed class XstsProperties
    {
        [JsonPropertyName("SandboxId")]
        public string SandboxId { get; set; } = "RETAIL";

        [JsonPropertyName("UserTokens")]
        public string[] UserTokens { get; set; } = Array.Empty<string>();
    }

    public sealed class XstsAuthorizeResponse
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
}
