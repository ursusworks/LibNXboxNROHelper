using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ursus.Xbox.Models
{

    public class GameStreamLoginResponse
    {
        public Offeringsettings offeringSettings { get; set; }
        public string market { get; set; }
        public string gsToken { get; set; }
        public string tokenType { get; set; }
        public int durationInSeconds { get; set; }
    }

    public class Offeringsettings
    {
        public bool allowRegionSelection { get; set; }
        public Region[] regions { get; set; }
        public object[] selectableServerTypes { get; set; }
        public Clientcloudsettings clientCloudSettings { get; set; }
    }

    public class Clientcloudsettings
    {
        public Environment[] Environments { get; set; }
    }

    public class Environment
    {
        public string Name { get; set; }
        public object AuthBaseUri { get; set; }
    }

    public class Region
    {
        public string name { get; set; }
        public string baseUri { get; set; }
        public string networkTestHostname { get; set; }
        public bool isDefault { get; set; }
        public object[] systemUpdateGroups { get; set; }
        public int fallbackPriority { get; set; }
    }

    public class SessionResponse
    {
        public string sessionId { get; set; }
        public string sessionPath { get; set; }
        public string state { get; set; }
    }

    public class SessionStateResponse
    {
        public string state { get; set; }
        public int detailedSessionState { get; set; }
        public SessionErrorDetails errorDetails { get; set; }

    }

    public class SessionErrorDetails
    {
        public string errorCode { get; set; }
        public string message { get; set; }
    }

    public class IceErrorDetails
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class ConfigurationResponse
    {
        public int keepAlivePulseInSeconds { get; set; }
        public ServerDetails serverDetails { get; set; }

    }

    public class ServerDetails
    {
        public string ipAddress { get; set; }
        public int port { get; set; }
        public string ipV4Address { get; set; }
        public int ipV4Port { get; set; }
        public string ipV6Address { get; set; }
        public int ipV6Port { get; set; }
        public string iceExchangePath { get; set; }
        public string[] stunServerAddresses { get; set; }
        public Srtp srtp { get; set; }

    }

    public class Srtp
    {
        public string key { get; set; }
    }

    public class IceConfig
    {
        [JsonPropertyName("Full")]
        public string Full { get; set; } = "1";

        [JsonPropertyName("PacingMs")]
        public string PacingMs { get; set; } = "50";

        [JsonPropertyName("Version")]
        public string Version { get; set; } = "1";

        [JsonPropertyName("Candidates")]
        public IceCandidates Candidates { get; set; } = new();

        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }
    }

    public class IceCandidates
    {
        [JsonPropertyName("count")]
        public string Count { get; set; }

        // Dynamic candidates:  "0", "1", "2", etc.
        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalCandidates { get; set; } = new();
    }

    public class IceCandidate
    {
        [JsonPropertyName("transportAddress")]
        public string TransportAddress { get; set; }

        [JsonPropertyName("baseAddress")]
        public string BaseAddress { get; set; }

        [JsonPropertyName("serverAddress")]
        public string ServerAddress { get; set; } = "";

        [JsonPropertyName("ipv6")]
        public string Ipv6 { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "3";

        [JsonPropertyName("addressType")]
        public string AddressType { get; set; }

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "2130705919";

        [JsonPropertyName("foundation")]
        public string Foundation { get; set; } = "1310976135";

        [JsonPropertyName("transport")]
        public string Transport { get; set; } = "udp";
    }

    public class IceExhangeRespone
    {
        public string exchangeResponse { get; set; }
        public IceErrorDetails errorDetails { get; set; }
    }

}
