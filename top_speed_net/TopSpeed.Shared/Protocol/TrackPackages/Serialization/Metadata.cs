using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        private static void WriteStringList(BinaryWriter writer, IReadOnlyList<string> values)
        {
            writer.Write(values.Count);
            for (var i = 0; i < values.Count; i++)
                writer.Write(values[i] ?? string.Empty);
        }

        private static string[] ReadStringList(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count < 0)
                throw new InvalidDataException("Invalid string list count.");

            var values = new string[count];
            for (var i = 0; i < count; i++)
                values[i] = reader.ReadString();
            return values;
        }

        private static void WriteMetadata(BinaryWriter writer, IReadOnlyDictionary<string, string> metadata)
        {
            var ordered = metadata.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
            writer.Write(ordered.Length);
            for (var i = 0; i < ordered.Length; i++)
            {
                writer.Write(ordered[i].Key ?? string.Empty);
                writer.Write(ordered[i].Value ?? string.Empty);
            }
        }

        private static IReadOnlyDictionary<string, string> ReadMetadata(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count < 0)
                throw new InvalidDataException("Invalid metadata count.");

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                if (!string.IsNullOrWhiteSpace(key))
                    map[key] = value ?? string.Empty;
            }

            return map;
        }
    }
}
