using System;
using System.IO;
using TopSpeed.Data;
using TopSpeed.Localization;

namespace TopSpeed.Protocol
{
    public static partial class PackageBuild
    {
        public static string ResolveTrackDisplayName(string selectedDisplayName, TrackData trackData, string trackFile)
        {
            var selected = (selectedDisplayName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(selected))
                return selected;

            var name = (trackData?.Name ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(name))
                return name;

            var directory = Path.GetDirectoryName(trackFile);
            var folder = string.IsNullOrWhiteSpace(directory) ? string.Empty : Path.GetFileName(directory);
            if (!string.IsNullOrWhiteSpace(folder))
                return folder;

            var fileName = Path.GetFileNameWithoutExtension(trackFile);
            return string.IsNullOrWhiteSpace(fileName) ? LocalizationService.Mark("Custom track") : fileName;
        }

        public static string ResolveTrackId(TrackData trackData, string trackFile)
        {
            var candidate = string.Empty;
            if (trackData?.Metadata != null
                && trackData.Metadata.TryGetValue("id", out var metadataId))
            {
                candidate = metadataId ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(candidate))
                candidate = trackData?.Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(candidate))
                candidate = Path.GetFileNameWithoutExtension(trackFile);
            if (string.IsNullOrWhiteSpace(candidate))
                candidate = "custom-track";

            var normalized = NormalizeTrackIdentifier(candidate);
            if (string.IsNullOrWhiteSpace(normalized))
                normalized = "custom-track";
            if (normalized.Length > ProtocolConstants.MaxTrackIdLength)
                normalized = normalized.Substring(0, ProtocolConstants.MaxTrackIdLength);
            return normalized;
        }

        public static string ResolveTrackVersion(TrackData trackData)
        {
            var version = (trackData?.Version ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(version))
                version = "1.0";
            if (version.Length > ProtocolConstants.MaxTrackVersionLength)
                version = version.Substring(0, ProtocolConstants.MaxTrackVersionLength);
            return version;
        }

        private static string NormalizeTrackIdentifier(string value)
        {
            var input = (value ?? string.Empty).Trim();
            if (input.Length == 0)
                return string.Empty;

            var buffer = new char[input.Length];
            var length = 0;
            var previousDash = false;
            for (var i = 0; i < input.Length; i++)
            {
                var ch = char.ToLowerInvariant(input[i]);
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '.')
                {
                    buffer[length++] = ch;
                    previousDash = false;
                    continue;
                }

                if (previousDash)
                    continue;

                buffer[length++] = '-';
                previousDash = true;
            }

            var normalized = new string(buffer, 0, length).Trim('-');
            return normalized;
        }
    }
}
