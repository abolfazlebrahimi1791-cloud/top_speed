using TopSpeed.Protocol;

namespace TopSpeed.Server.Protocol
{
    internal static partial class PacketSerializer
    {
        private static int MeasureCatalogTrackRef(TrackPackageRef track)
        {
            var normalized = NormalizeCatalogTrackRef(track);
            return 1
                + 2 + PacketWriter.MeasureString16(normalized.BuiltInTrackKey)
                + 2 + PacketWriter.MeasureString16(normalized.TrackId)
                + 2 + PacketWriter.MeasureString16(normalized.Version)
                + 2 + PacketWriter.MeasureString16(normalized.Hash);
        }

        private static void WriteCatalogTrackRef(ref PacketWriter writer, TrackPackageRef track)
        {
            var normalized = NormalizeCatalogTrackRef(track);
            writer.WriteByte((byte)normalized.Kind);
            writer.WriteString16(normalized.BuiltInTrackKey ?? string.Empty);
            writer.WriteString16(normalized.TrackId ?? string.Empty);
            writer.WriteString16(normalized.Version ?? string.Empty);
            writer.WriteString16(normalized.Hash ?? string.Empty);
        }

        private static TrackPackageRef NormalizeCatalogTrackRef(TrackPackageRef track)
        {
            return TrackPackageRef.Clone(track);
        }
    }
}
