using System;
using System.Collections.Generic;
using System.Globalization;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        public static bool TryValidate(TrackPackagePayload payload, out string error)
        {
            error = string.Empty;
            if (payload == null || payload.Manifest == null)
            {
                error = "Track package payload is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.Manifest.TrackId))
            {
                error = "Track package track id is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.Manifest.Version))
            {
                error = "Track package version is required.";
                return false;
            }

            if (payload.Definitions == null || payload.Definitions.Length == 0)
            {
                error = "Track package definitions are required.";
                return false;
            }

            if (payload.Definitions.Length > ProtocolConstants.MaxMultiTrackLength)
            {
                error = "Track package exceeds maximum definition count.";
                return false;
            }

            var weatherProfiles = payload.WeatherProfiles ?? new Dictionary<string, TopSpeed.Data.TrackWeatherProfile>(StringComparer.OrdinalIgnoreCase);
            var metadata = payload.Metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var roomProfiles = payload.RoomProfiles ?? new Dictionary<string, TopSpeed.Data.TrackRoomDefinition>(StringComparer.OrdinalIgnoreCase);
            var defaultWeather = string.IsNullOrWhiteSpace(payload.Manifest.DefaultWeatherProfileId)
                ? TopSpeed.Data.TrackWeatherProfile.DefaultProfileId
                : payload.Manifest.DefaultWeatherProfileId.Trim();
            if (!weatherProfiles.ContainsKey(defaultWeather))
            {
                error = "Track package default weather profile is missing.";
                return false;
            }

            foreach (var pair in metadata)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    error = "Track package contains an invalid metadata key.";
                    return false;
                }
            }

            foreach (var pair in roomProfiles)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    error = "Track package contains an invalid room profile id.";
                    return false;
                }

                var room = pair.Value;
                if (room == null)
                {
                    error = string.Format(CultureInfo.InvariantCulture, "Track package room profile '{0}' is invalid.", pair.Key);
                    return false;
                }
            }

            var assets = payload.AssetBlobs ?? new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var sounds = payload.SoundDefinitions ?? new Dictionary<string, TopSpeed.Data.TrackSoundSourceDefinition>(StringComparer.OrdinalIgnoreCase);
            var definitions = payload.Definitions ?? Array.Empty<TopSpeed.Data.TrackDefinition>();
            var normalizedAssetKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            long totalAssets = 0;
            foreach (var pair in assets)
            {
                var key = NormalizeAssetKey(pair.Key ?? string.Empty);
                if (string.IsNullOrWhiteSpace(key))
                {
                    error = "Track package contains an invalid asset blob key.";
                    return false;
                }
                normalizedAssetKeys.Add(key);

                var bytes = pair.Value ?? Array.Empty<byte>();
                if (bytes.Length > ProtocolConstants.MaxTrackPackageAssetBytes)
                {
                    error = string.Format(CultureInfo.InvariantCulture, "Asset '{0}' exceeds max size.", key);
                    return false;
                }

                totalAssets += bytes.Length;
                if (totalAssets > ProtocolConstants.MaxTrackPackageBytes)
                {
                    error = "Track package exceeds max size.";
                    return false;
                }
            }

            foreach (var pair in sounds)
            {
                var sound = pair.Value;
                if (sound == null)
                {
                    error = "Track package contains an invalid sound definition.";
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(sound.Path) && !normalizedAssetKeys.Contains(NormalizeAssetKey(sound.Path ?? string.Empty)))
                {
                    error = string.Format(CultureInfo.InvariantCulture, "Sound '{0}' references missing asset path '{1}'.", pair.Key, sound.Path);
                    return false;
                }

                for (var i = 0; i < sound.VariantPaths.Count; i++)
                {
                    if (!normalizedAssetKeys.Contains(NormalizeAssetKey(sound.VariantPaths[i] ?? string.Empty)))
                    {
                        error = string.Format(CultureInfo.InvariantCulture, "Sound '{0}' references missing variant asset path '{1}'.", pair.Key, sound.VariantPaths[i]);
                        return false;
                    }
                }
            }

            for (var i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (!string.IsNullOrWhiteSpace(definition.WeatherProfileId)
                    && !weatherProfiles.ContainsKey(definition.WeatherProfileId!))
                {
                    error = string.Format(CultureInfo.InvariantCulture, "Track definition {0} references missing weather profile '{1}'.", i, definition.WeatherProfileId);
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(definition.RoomId)
                    && !roomProfiles.ContainsKey(definition.RoomId!)
                    && !TopSpeed.Data.TrackRoomLibrary.TryGetPreset(definition.RoomId!, out _))
                {
                    error = string.Format(CultureInfo.InvariantCulture, "Track definition {0} references missing room profile '{1}'.", i, definition.RoomId);
                    return false;
                }

                for (var sourceIndex = 0; sourceIndex < definition.SoundSourceIds.Count; sourceIndex++)
                {
                    var soundSourceId = definition.SoundSourceIds[sourceIndex];
                    if (string.IsNullOrWhiteSpace(soundSourceId))
                        continue;
                    if (!sounds.ContainsKey(soundSourceId))
                    {
                        error = string.Format(CultureInfo.InvariantCulture, "Track definition {0} references missing sound source '{1}'.", i, soundSourceId);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
