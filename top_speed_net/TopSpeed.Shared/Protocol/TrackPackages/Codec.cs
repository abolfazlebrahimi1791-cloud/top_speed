using System;
using System.Collections.Generic;
using System.IO;
using TopSpeed.Data;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        public static byte[] Serialize(TrackPackagePayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                WritePayload(writer, payload, includeHash: true);
                writer.Flush();
                return ms.ToArray();
            }
        }

        public static bool TryDeserialize(byte[] bytes, out TrackPackagePayload payload, out string error)
        {
            payload = new TrackPackagePayload();
            error = string.Empty;

            if (bytes == null || bytes.Length == 0)
            {
                error = "Track package payload is empty.";
                return false;
            }

            try
            {
                using (var ms = new MemoryStream(bytes, writable: false))
                using (var reader = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    payload = ReadPayload(reader);
                    if (ms.Position != ms.Length)
                    {
                        error = "Track package payload contains trailing bytes.";
                        return false;
                    }
                }
            }
            catch (Exception ex) when (ex is EndOfStreamException || ex is IOException || ex is InvalidDataException || ex is ArgumentException)
            {
                error = ex.Message;
                payload = new TrackPackagePayload();
                return false;
            }

            return TryValidate(payload, out error);
        }

        public static TrackData ToTrackData(TrackPackagePayload payload, bool userDefined, string sourcePath)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var manifest = payload.Manifest ?? new TrackPackageManifest();
            var name = string.IsNullOrWhiteSpace(manifest.TrackId) ? "custom" : manifest.TrackId;
            return new TrackData(
                userDefined,
                string.IsNullOrWhiteSpace(manifest.DefaultWeatherProfileId) ? TrackWeatherProfile.DefaultProfileId : manifest.DefaultWeatherProfileId,
                payload.WeatherProfiles,
                manifest.Ambience,
                payload.Definitions,
                manifest.Laps,
                name,
                manifest.Version,
                metadata: payload.Metadata,
                roomProfiles: payload.RoomProfiles,
                soundSources: payload.SoundDefinitions,
                sourcePath: sourcePath);
        }

        public static string NormalizeAssetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var normalized = key.Trim().Replace('\\', '/').TrimStart('/');
            if (normalized.Length == 0 || normalized.IndexOf(':') >= 0)
                return string.Empty;

            var segments = normalized.Split('/');
            for (var i = 0; i < segments.Length; i++)
            {
                if (segments[i] == "." || segments[i] == ".." || segments[i].Length == 0)
                    return string.Empty;
            }

            return normalized;
        }
    }
}
