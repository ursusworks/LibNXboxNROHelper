using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ursus.Xbox.Models;

namespace Ursus.Xbox.Helpers;

public class XboxRequestSigner
{
    private readonly ECDsa _ecdsa;

    public XboxRequestSigner()
    {
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    }

    public ProofKey GetProofKey()
    {
        ECParameters parameters = _ecdsa.ExportParameters(includePrivateParameters: false);

        return new ProofKey
        {
            X = Base64UrlEncode(parameters.Q.X),
            Y = Base64UrlEncode(parameters.Q.Y)
        };
    }

    public async Task<HttpRequestMessage> SignRequestAsync(
        HttpRequestMessage request,
        int signingPolicyVersion = 1,
        int maxBodyBytes = 8192)
    {
        long fileTime = DateTimeToFileTime(DateTime.UtcNow);
        byte[] fileTimeBytes = BitConverter.GetBytes(fileTime);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(fileTimeBytes);
        }

        byte[] versionBytes = BitConverter.GetBytes(signingPolicyVersion);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(versionBytes);
        }

        byte[] bodyBytes = Array.Empty<byte>();
        if (request.Content != null)
        {
            bodyBytes = await request.Content.ReadAsByteArrayAsync();
            var newContent = new ByteArrayContent(bodyBytes);
            foreach (var header in request.Content.Headers)
            {
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            request.Content = newContent;
        }

        byte[] messageToSign = BuildSigningMessage(
            versionBytes,
            fileTimeBytes,
            request,
            bodyBytes,
            maxBodyBytes);

        byte[] hash = SHA256.HashData(messageToSign);

        byte[] signature = _ecdsa.SignHash(hash, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        byte[] finalSignature = versionBytes
            .Concat(fileTimeBytes)
            .Concat(signature)
            .ToArray();

        string signatureHeader = Convert.ToBase64String(finalSignature);
        request.Headers.TryAddWithoutValidation("Signature", signatureHeader);

        return request;
    }

    private static byte[] BuildSigningMessage(
        byte[] versionBytes,
        byte[] timestampBytes,
        HttpRequestMessage request,
        byte[] bodyBytes,
        int maxBodyBytes)
    {
        using var stream = new System.IO.MemoryStream();
        using var writer = new System.IO.BinaryWriter(stream);

        writer.Write(versionBytes);
        writer.Write((byte)0);
        writer.Write(timestampBytes);
        writer.Write((byte)0);

        string method = request.Method.Method.ToUpperInvariant();
        writer.Write(Encoding.UTF8.GetBytes(method));
        writer.Write((byte)0);

        string pathAndQuery = request.RequestUri.PathAndQuery;
        writer.Write(Encoding.UTF8.GetBytes(pathAndQuery));
        writer.Write((byte)0);

        string authorization = "";
        if (request.Headers.TryGetValues("Authorization", out var authValues))
        {
            authorization = authValues.FirstOrDefault() ?? "";
        }
        writer.Write(Encoding.UTF8.GetBytes(authorization));
        writer.Write((byte)0);

        if (bodyBytes.Length > 0)
        {
            int bytesToWrite = Math.Min(bodyBytes.Length, maxBodyBytes);
            writer.Write(bodyBytes, 0, bytesToWrite);
        }
        writer.Write((byte)0);

        return stream.ToArray();
    }

    private long DateTimeToFileTime(DateTime dateTime)
    {
        DateTime utc = dateTime.ToUniversalTime();
        return utc.ToFileTimeUtc();
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}