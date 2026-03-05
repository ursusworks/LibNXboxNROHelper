using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ursus.Xbox.Helpers
{
    public class Base64Url
    {

        public string B64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        // Windows FILETIME 100ns ticks since 1601-01-01 UTC, big-endian 64-bit
        static long GetWindowsFileTimeUtc() => DateTime.UtcNow.ToFileTimeUtc();

        // Build the canonical buffer that Xbox signs for the Signature header.
        // Layout (community reverse-engineered & used by Xbox clients):
        // int32(1, big-endian) | 0x00 | int64(windowsFileTime, big-endian) | 0x00 |
        // ASCII(method) | 0x00 | ASCII(pathAndQuery) | 0x00 |
        // ASCII(authorizationHeaderOrEmpty) | 0x00 | UTF8(body) | 0x00
        public static byte[] BuildSignaturePayload(string method, string pathAndQuery, string authorization, string bodyJson)
        {
            var methodBytes = Encoding.ASCII.GetBytes(method.ToUpperInvariant());
            var pathBytes = Encoding.ASCII.GetBytes(pathAndQuery);
            var authBytes = Encoding.ASCII.GetBytes(authorization ?? string.Empty);
            var bodyBytes = Encoding.UTF8.GetBytes(bodyJson ?? string.Empty);

            // Compute size
            int size =
                4 + 1 + 8 + 1 + // version + sep + filetime + sep
                methodBytes.Length + 1 +
                pathBytes.Length + 1 +
                authBytes.Length + 1 +
                bodyBytes.Length + 1;

            var buffer = new byte[size];
            int offset = 0;

            // version (1) big-endian
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset, 4), 1);
            offset += 4;

            buffer[offset++] = 0x00;

            // FILETIME big-endian
            long filetime = GetWindowsFileTimeUtc();
            // big-endian 64-bit
            Span<byte> ft = stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(ft, filetime);
            ft.CopyTo(buffer.AsSpan(offset, 8));
            offset += 8;

            buffer[offset++] = 0x00;

            methodBytes.CopyTo(buffer.AsSpan(offset));
            offset += methodBytes.Length;
            buffer[offset++] = 0x00;

            pathBytes.CopyTo(buffer.AsSpan(offset));
            offset += pathBytes.Length;
            buffer[offset++] = 0x00;

            authBytes.CopyTo(buffer.AsSpan(offset));
            offset += authBytes.Length;
            buffer[offset++] = 0x00;

            bodyBytes.CopyTo(buffer.AsSpan(offset));
            offset += bodyBytes.Length;
            buffer[offset++] = 0x00;

            return buffer;
        }

    }
}
