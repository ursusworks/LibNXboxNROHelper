using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ursus.Xbox.Models
{





    public class DeviceAuthRequest
    {
        public string RelyingParty { get; set; } = "http://auth.xboxlive.com";
        public string TokenType { get; set; } = "JWT";
        public PropertiesObj Properties { get; set; }
        public class PropertiesObj
        {
            public string AuthMethod { get; set; } = "ProofOfPossession";
            public string Id { get; set; }              // e.g. "DECF45E4-945D-4379-B708-D4EE92C12D99"
            public string DeviceType { get; set; }      // e.g. "Android"
            public string Version { get; set; }         // e.g. "15.6.1"
            public ProofKey ProofKey { get; set; }
        }
    }


    public class DeviceTokenRequest
    {
        public string RelyingParty { get; set; }
        public string TokenType { get; set; }
        public DeviceProperties Properties { get; set; }
    }

    public class DeviceProperties
    {
        public string AuthMethod { get; set; }
        public string Id { get; set; }
        public string DeviceType { get; set; }
        public string Version { get; set; }
        public string ProofKey { get; set; }
    }

    public class DeviceToken
    {
        public string Token { get; set; }
        public DateTime IssueInstant { get; set; }
        public DateTime NotAfter { get; set; }
    }

    public enum DeviceType
    {
        Unknown,
        XboxOne,
        Xbox360,
        WindowsDesktop,
        WindowsStore,
        WindowsPhone,
        iPhone,
        iPad,
        Android
    }
}
