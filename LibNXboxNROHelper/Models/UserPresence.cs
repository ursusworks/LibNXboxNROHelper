using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ursus.Xbox.Models
{
    public class UserPresence
    {
        public string xuid { get; set; }
        public string state { get; set; }
        public Device[] devices { get; set; }
        public bool cloaked { get; set; }
        public LastSeen lastSeen { get; set; }
    }

    public class Device
    {
        public string type { get; set; }
        public Title[] titles { get; set; }
    }

    public class Title
    {
        public string id { get; set; }
        [JsonPropertyName("name")]
        public string name { get; set; }
        public Activity activity { get; set; }
        public string placement { get; set; }
        public string state { get; set; }
        public DateTime lastModified { get; set; }
    }

    public class Activity
    {
        public string richPresence { get; set; }
        public string media { get; set; }

    }

    public class LastSeen
    {
        public string deviceType { get; set; }
        public string titleId { get; set; }
        public string titleName { get; set; }
        public DateTime timestamp { get; set; }

    }

}
