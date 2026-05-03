using System;

namespace TopSpeed.Protocol
{
    public static partial class PacketValidation
    {
        public static bool IsValidTrackPackageRef(TrackPackageRef track)
        {
            if (track == null)
                return false;

            if (track.Kind == RoomTrackSelectionKind.BuiltIn)
                return !string.IsNullOrWhiteSpace(track.BuiltInTrackKey);

            if (track.Kind != RoomTrackSelectionKind.CustomPackage)
                return false;

            if (string.IsNullOrWhiteSpace(track.TrackId) || track.TrackId.Length > ProtocolConstants.MaxTrackIdLength)
                return false;
            if (string.IsNullOrWhiteSpace(track.Version) || track.Version.Length > ProtocolConstants.MaxTrackVersionLength)
                return false;
            if (string.IsNullOrWhiteSpace(track.Hash) || track.Hash.Length > ProtocolConstants.MaxTrackHashLength)
                return false;

            return true;
        }

        public static bool IsValidTrackPackageUploadBegin(PacketTrackPackageUploadBegin packet)
        {
            return packet != null
                && packet.UploadId != 0
                && !string.IsNullOrWhiteSpace(packet.TrackId)
                && packet.TrackId.Length <= ProtocolConstants.MaxTrackIdLength
                && !string.IsNullOrWhiteSpace(packet.Version)
                && packet.Version.Length <= ProtocolConstants.MaxTrackVersionLength
                && !string.IsNullOrWhiteSpace(packet.Hash)
                && packet.Hash.Length <= ProtocolConstants.MaxTrackHashLength
                && packet.TotalBytes > 0
                && packet.TotalBytes <= ProtocolConstants.MaxTrackPackageBytes;
        }

        public static bool IsValidTrackPackageUploadChunk(PacketTrackPackageUploadChunk packet)
        {
            return packet != null
                && packet.UploadId != 0
                && packet.Data != null
                && packet.Data.Length > 0
                && packet.Data.Length <= ProtocolConstants.MaxTrackPackageChunkBytes;
        }

        public static bool IsValidTrackPackageUploadEnd(PacketTrackPackageUploadEnd packet)
        {
            return packet != null && packet.UploadId != 0;
        }

        public static bool IsValidTrackPackageUploadResult(PacketTrackPackageUploadResult packet)
        {
            return packet != null
                && packet.UploadId != 0
                && packet.Status >= TrackPackageUploadStatus.Accepted
                && packet.Status <= TrackPackageUploadStatus.Rejected;
        }

        public static bool IsValidTrackPackageTransferBegin(PacketTrackPackageTransferBegin packet)
        {
            return packet != null
                && !string.IsNullOrWhiteSpace(packet.TrackId)
                && packet.TrackId.Length <= ProtocolConstants.MaxTrackIdLength
                && !string.IsNullOrWhiteSpace(packet.Version)
                && packet.Version.Length <= ProtocolConstants.MaxTrackVersionLength
                && !string.IsNullOrWhiteSpace(packet.Hash)
                && packet.Hash.Length <= ProtocolConstants.MaxTrackHashLength
                && packet.TotalBytes > 0
                && packet.TotalBytes <= ProtocolConstants.MaxTrackPackageBytes;
        }

        public static bool IsValidTrackPackageTransferChunk(PacketTrackPackageTransferChunk packet)
        {
            return packet != null
                && !string.IsNullOrWhiteSpace(packet.Hash)
                && packet.Hash.Length <= ProtocolConstants.MaxTrackHashLength
                && packet.Data != null
                && packet.Data.Length > 0
                && packet.Data.Length <= ProtocolConstants.MaxTrackPackageChunkBytes;
        }

        public static bool IsValidTrackPackageTransferEnd(PacketTrackPackageTransferEnd packet)
        {
            return packet != null
                && !string.IsNullOrWhiteSpace(packet.Hash)
                && packet.Hash.Length <= ProtocolConstants.MaxTrackHashLength;
        }

        public static bool IsValidTrackPackageReady(PacketTrackPackageReady packet)
        {
            return packet != null
                && !string.IsNullOrWhiteSpace(packet.Hash)
                && packet.Hash.Length <= ProtocolConstants.MaxTrackHashLength;
        }

        public static bool IsValidTrackPackageCatalogRequest(PacketTrackPackageCatalogRequest packet)
        {
            return packet != null;
        }

        public static bool IsValidTrackPackageCatalogEntry(PacketTrackPackageCatalogEntry entry)
        {
            return entry != null
                && entry.Track != null
                && entry.Track.IsCustomPackage
                && IsValidTrackPackageRef(entry.Track)
                && !string.IsNullOrWhiteSpace(entry.DisplayName)
                && entry.DisplayName.Length <= ProtocolConstants.MaxTrackPackageDisplayNameLength;
        }

        public static bool IsValidTrackPackageCatalog(PacketTrackPackageCatalog packet)
        {
            if (packet == null || packet.Tracks == null)
                return false;

            if (packet.Tracks.Length > ProtocolConstants.MaxTrackPackageCatalogEntries)
                return false;

            for (var i = 0; i < packet.Tracks.Length; i++)
            {
                if (!IsValidTrackPackageCatalogEntry(packet.Tracks[i]))
                    return false;
            }

            return true;
        }
    }
}
