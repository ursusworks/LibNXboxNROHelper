using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ursus.Xbox.Models;

namespace Ursus.Xbox.Helpers
{
    public class ProofKeyGenerator
    {
        private ECDsa _ecdsa;

        public ProofKeyGenerator()
        {
            // Create a new P-256 (secp256r1 / prime256v1) keypair
            _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        }

        public ProofKey GetProofKey()
        {
            // Export the public key parameters
            ECParameters parameters = _ecdsa.ExportParameters(includePrivateParameters: false);

            // Convert X and Y coordinates to Base64Url (no padding)
            string x = Base64UrlEncode(parameters.Q.X);
            string y = Base64UrlEncode(parameters.Q.Y);

            return new ProofKey
            {
                X = x,
                Y = y
            };
        }

        public string GetProofKeyJson()
        {
            var proofKey = GetProofKey();
            return JsonSerializer.Serialize(proofKey, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            });
        }

        // Sign request data using the private key (for Xbox Web Signature)
        public byte[] SignData(byte[] data)
        {
            return _ecdsa.SignData(data, HashAlgorithmName.SHA256);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            // Convert to Base64Url format (RFC 4648)
            return Convert.ToBase64String(input)
                .TrimEnd('=')                    // Remove padding
                .Replace('+', '-')               // Replace + with -
                .Replace('/', '_');              // Replace / with _
        }

        // Optional: Export/Import for persistence
        public string ExportPrivateKey()
        {
            ECParameters parameters = _ecdsa.ExportParameters(includePrivateParameters: true);
            return Convert.ToBase64String(_ecdsa.ExportECPrivateKey());
        }

        public void ImportPrivateKey(string base64Key)
        {
            byte[] keyData = Convert.FromBase64String(base64Key);
            _ecdsa.ImportECPrivateKey(keyData, out _);
        }
    }
}
