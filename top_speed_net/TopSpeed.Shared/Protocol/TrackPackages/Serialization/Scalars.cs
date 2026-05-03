using System;
using System.IO;
using System.Numerics;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        private static void WriteNullableSingle(BinaryWriter writer, float? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static float? ReadNullableSingle(BinaryReader reader)
        {
            return reader.ReadBoolean() ? reader.ReadSingle() : (float?)null;
        }

        private static void WriteNullableVector3(BinaryWriter writer, Vector3? value)
        {
            writer.Write(value.HasValue);
            if (!value.HasValue)
                return;
            writer.Write(value.Value.X);
            writer.Write(value.Value.Y);
            writer.Write(value.Value.Z);
        }

        private static Vector3? ReadNullableVector3(BinaryReader reader)
        {
            if (!reader.ReadBoolean())
                return null;
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        private static string? NormalizeNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
