using System;
using System.Collections.Generic;
using TopSpeed.Data;

namespace TopSpeed.Protocol
{
    public sealed class TrackPackagePayload
    {
        public TrackPackageManifest Manifest = new TrackPackageManifest();
        public TrackDefinition[] Definitions = Array.Empty<TrackDefinition>();
        public IReadOnlyDictionary<string, string> Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyDictionary<string, TrackRoomDefinition> RoomProfiles = new Dictionary<string, TrackRoomDefinition>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyDictionary<string, TrackWeatherProfile> WeatherProfiles = new Dictionary<string, TrackWeatherProfile>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyDictionary<string, TrackSoundSourceDefinition> SoundDefinitions = new Dictionary<string, TrackSoundSourceDefinition>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyDictionary<string, byte[]> AssetBlobs = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
    }
}
