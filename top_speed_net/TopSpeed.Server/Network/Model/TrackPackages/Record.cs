using System;
using TopSpeed.Data;
using TopSpeed.Protocol;

namespace TopSpeed.Server.Network
{
    internal sealed class PackageRecord
    {
        public TrackPackageRef Ref { get; set; } = new TrackPackageRef();
        public TrackPackagePayload Payload { get; set; } = new TrackPackagePayload();
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        public TrackData TrackData { get; set; } = new TrackData(false, TrackWeather.Sunny, TrackAmbience.NoAmbience, Array.Empty<TrackDefinition>());
        public DateTime LastAccessUtc { get; set; } = DateTime.UtcNow;
        public bool FromServerTracksFolder { get; set; }
        public string SourcePath { get; set; } = string.Empty;
        public DateTime SourceLastWriteUtc { get; set; } = DateTime.MinValue;
    }
}

