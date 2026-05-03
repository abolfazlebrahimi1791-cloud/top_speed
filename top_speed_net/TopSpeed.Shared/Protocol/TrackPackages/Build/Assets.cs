using System;
using System.Collections.Generic;
using System.IO;
using TopSpeed.Data;
using TopSpeed.Localization;

namespace TopSpeed.Protocol
{
    public static partial class PackageBuild
    {
        public static bool TryBuildTrackAssetBlobs(TrackData trackData, out IReadOnlyDictionary<string, byte[]> assets, out string error)
        {
            assets = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            error = string.Empty;

            var sounds = trackData.SoundSources ?? new Dictionary<string, TrackSoundSourceDefinition>(StringComparer.OrdinalIgnoreCase);
            if (sounds.Count == 0)
                return true;

            var sourcePath = trackData.SourcePath ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                error = LocalizationService.Mark("Track source path is missing, so sound assets cannot be resolved.");
                return false;
            }

            var trackRoot = Path.GetDirectoryName(Path.GetFullPath(sourcePath));
            if (string.IsNullOrWhiteSpace(trackRoot))
            {
                error = LocalizationService.Mark("Unable to resolve custom track folder path.");
                return false;
            }

            var map = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in sounds)
            {
                var sound = pair.Value;
                if (sound == null)
                    continue;

                if (!TryAddTrackAsset(trackRoot, sound.Path, map, out error))
                    return false;

                var variants = sound.VariantPaths ?? Array.Empty<string>();
                for (var i = 0; i < variants.Count; i++)
                {
                    if (!TryAddTrackAsset(trackRoot, variants[i], map, out error))
                        return false;
                }
            }

            assets = map;
            return true;
        }

        private static bool TryAddTrackAsset(string trackRoot, string? relativeAssetPath, Dictionary<string, byte[]> map, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(relativeAssetPath))
                return true;

            var key = TrackPackageCodec.NormalizeAssetKey(relativeAssetPath ?? string.Empty);
            if (string.IsNullOrWhiteSpace(key))
            {
                error = LocalizationService.Format(LocalizationService.Mark("Invalid sound asset path: {0}"), relativeAssetPath ?? string.Empty);
                return false;
            }

            if (map.ContainsKey(key))
                return true;

            var relativePath = key.Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.GetFullPath(Path.Combine(trackRoot, relativePath));
            if (!IsPathInsideRoot(absolutePath, trackRoot))
            {
                error = LocalizationService.Format(LocalizationService.Mark("Sound asset path escapes the track folder: {0}"), relativeAssetPath ?? string.Empty);
                return false;
            }

            if (!File.Exists(absolutePath))
            {
                error = LocalizationService.Format(LocalizationService.Mark("Missing sound asset file: {0}"), relativeAssetPath ?? string.Empty);
                return false;
            }

            try
            {
                map[key] = File.ReadAllBytes(absolutePath);
                return true;
            }
            catch (IOException ex)
            {
                error = ex.Message;
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool IsPathInsideRoot(string candidatePath, string rootPath)
        {
            if (string.Equals(candidatePath, rootPath, StringComparison.OrdinalIgnoreCase))
                return true;

            var rootPrefix = rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return candidatePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
