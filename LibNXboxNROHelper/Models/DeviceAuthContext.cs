namespace Ursus.Xbox.Models
{
    public sealed class DeviceAuthContext
    {
        public string DeviceId { get; init; } = string.Empty;
        public string DeviceType { get; init; } = "Win32";
        public string SerialNumber { get; init; } = "SN000000000";
        public string FirmwareVersion { get; init; } = "0.0.0";
        public object PublicJwk { get; init; } = default!;

        /// <summary>Return the PoP signature header value for the request body.</summary>
        public Func<DeviceAuthenticateRequest, string> SignPayload { get; init; } = _ => string.Empty;
    }
}