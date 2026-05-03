using System;

namespace TopSpeed.Server.Network
{
    internal sealed class PackageUploadSession
    {
        public uint UploadId { get; set; }
        public uint OwnerPlayerId { get; set; }
        public uint RoomId { get; set; }
        public string TrackId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public uint TotalBytes { get; set; }
        public ushort NextChunkIndex { get; set; }
        public int Offset { get; set; }
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
    }
}

