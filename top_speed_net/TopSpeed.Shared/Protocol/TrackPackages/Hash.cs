using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        public static string ComputeHash(TrackPackagePayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                WritePayload(writer, payload, includeHash: false);
                writer.Flush();
                return ComputeHash(ms.ToArray());
            }
        }

        public static string ComputeHash(byte[] canonicalBytes)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(canonicalBytes ?? Array.Empty<byte>());
                var builder = new StringBuilder(hash.Length * 2);
                for (var i = 0; i < hash.Length; i++)
                    builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }
    }
}
