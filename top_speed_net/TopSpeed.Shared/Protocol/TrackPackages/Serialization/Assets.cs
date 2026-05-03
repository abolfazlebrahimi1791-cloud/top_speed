using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        private static void WriteAssets(BinaryWriter writer, IReadOnlyDictionary<string, byte[]> assets)
        {
            var ordered = assets.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
            writer.Write(ordered.Length);
            for (var i = 0; i < ordered.Length; i++)
            {
                writer.Write(NormalizeAssetKey(ordered[i].Key));
                var data = ordered[i].Value ?? Array.Empty<byte>();
                writer.Write(data.Length);
                writer.Write(data);
            }
        }

        private static IReadOnlyDictionary<string, byte[]> ReadAssets(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count < 0)
                throw new InvalidDataException("Invalid asset count.");

            var map = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < count; i++)
            {
                var key = NormalizeAssetKey(reader.ReadString());
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidDataException("Invalid asset key.");
                var length = reader.ReadInt32();
                if (length < 0)
                    throw new InvalidDataException("Invalid asset blob length.");
                var data = reader.ReadBytes(length);
                if (data.Length != length)
                    throw new EndOfStreamException("Unexpected end of asset blob.");
                map[key] = data;
            }

            return map;
        }
    }
}
