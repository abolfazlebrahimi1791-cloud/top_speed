using System;
using TopSpeed.Protocol;

namespace TopSpeed.Server.Protocol
{
    internal static partial class PacketSerializer
    {
        public static bool TryReadTrackPackageUploadBegin(byte[] data, out PacketTrackPackageUploadBegin packet)
        {
            packet = new PacketTrackPackageUploadBegin();
            if (data.Length < 2 + 4 + 2 + 2 + 2 + 4)
                return false;
            if (data[0] != ProtocolConstants.Version || data[1] != (byte)Command.TrackPackageUploadBegin)
                return false;

            try
            {
                var reader = new PacketReader(data);
                reader.ReadByte();
                reader.ReadByte();
                packet.UploadId = reader.ReadUInt32();
                packet.TrackId = reader.ReadString16();
                packet.Version = reader.ReadString16();
                packet.Hash = TrackPackageRef.NormalizeHash(reader.ReadString16());
                packet.TotalBytes = reader.ReadUInt32();
                return PacketValidation.IsValidTrackPackageUploadBegin(packet);
            }
            catch
            {
                packet = new PacketTrackPackageUploadBegin();
                return false;
            }
        }

        public static bool TryReadTrackPackageUploadChunk(byte[] data, out PacketTrackPackageUploadChunk packet)
        {
            packet = new PacketTrackPackageUploadChunk();
            if (data.Length < 2 + 4 + 2 + 2)
                return false;
            if (data[0] != ProtocolConstants.Version || data[1] != (byte)Command.TrackPackageUploadChunk)
                return false;

            try
            {
                var reader = new PacketReader(data);
                reader.ReadByte();
                reader.ReadByte();
                packet.UploadId = reader.ReadUInt32();
                packet.ChunkIndex = reader.ReadUInt16();
                var length = reader.ReadUInt16();
                if (length == 0 || length > ProtocolConstants.MaxTrackPackageChunkBytes)
                    return false;
                if (data.Length != 2 + 4 + 2 + 2 + length)
                    return false;

                var bytes = new byte[length];
                for (var i = 0; i < length; i++)
                    bytes[i] = reader.ReadByte();
                packet.Data = bytes;
                return PacketValidation.IsValidTrackPackageUploadChunk(packet);
            }
            catch
            {
                packet = new PacketTrackPackageUploadChunk();
                return false;
            }
        }

        public static bool TryReadTrackPackageUploadEnd(byte[] data, out PacketTrackPackageUploadEnd packet)
        {
            packet = new PacketTrackPackageUploadEnd();
            if (data.Length < 2 + 4)
                return false;
            if (data[0] != ProtocolConstants.Version || data[1] != (byte)Command.TrackPackageUploadEnd)
                return false;
            var reader = new PacketReader(data);
            reader.ReadByte();
            reader.ReadByte();
            packet.UploadId = reader.ReadUInt32();
            return PacketValidation.IsValidTrackPackageUploadEnd(packet);
        }

        public static bool TryReadTrackPackageReady(byte[] data, out PacketTrackPackageReady packet)
        {
            packet = new PacketTrackPackageReady();
            if (data.Length < 2 + 2)
                return false;
            if (data[0] != ProtocolConstants.Version || data[1] != (byte)Command.TrackPackageReady)
                return false;
            try
            {
                var reader = new PacketReader(data);
                reader.ReadByte();
                reader.ReadByte();
                packet.Hash = TrackPackageRef.NormalizeHash(reader.ReadString16());
                return PacketValidation.IsValidTrackPackageReady(packet);
            }
            catch
            {
                packet = new PacketTrackPackageReady();
                return false;
            }
        }

        public static bool TryReadTrackPackageCatalogRequest(byte[] data, out PacketTrackPackageCatalogRequest packet)
        {
            packet = new PacketTrackPackageCatalogRequest();
            if (data.Length != 2)
                return false;
            if (data[0] != ProtocolConstants.Version || data[1] != (byte)Command.TrackPackageCatalogRequest)
                return false;
            return PacketValidation.IsValidTrackPackageCatalogRequest(packet);
        }
    }
}
