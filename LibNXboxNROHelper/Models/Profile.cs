using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ursus.Xbox.Models
{
    public sealed class ProfileSettingsResponse
    {
        [JsonPropertyName("profileUsers")]
        public List<ProfileUser> ProfileUsers { get; set; } = new();
    }

    public sealed class ProfileUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("settings")]
        public List<ProfileSetting> Settings { get; set; } = new();

        [JsonPropertyName("isSponsoredUser")]
        public bool IsSponsoredUser { get; set; }
    }

    public sealed class ProfileSetting
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}
