using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        private static void WriteSoundDefinitions(BinaryWriter writer, IReadOnlyDictionary<string, TopSpeed.Data.TrackSoundSourceDefinition> sounds)
        {
            var ordered = sounds.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
            writer.Write(ordered.Length);
            for (var i = 0; i < ordered.Length; i++)
            {
                var sound = ordered[i].Value;
                writer.Write(sound.Id ?? string.Empty);
                writer.Write((byte)sound.Type);
                writer.Write(sound.Path ?? string.Empty);
                writer.Write(sound.VariantPaths.Count);
                for (var variantIndex = 0; variantIndex < sound.VariantPaths.Count; variantIndex++)
                    writer.Write(sound.VariantPaths[variantIndex] ?? string.Empty);
                writer.Write(sound.VariantSourceIds.Count);
                for (var sourceIndex = 0; sourceIndex < sound.VariantSourceIds.Count; sourceIndex++)
                    writer.Write(sound.VariantSourceIds[sourceIndex] ?? string.Empty);
                writer.Write((byte)sound.RandomMode);
                writer.Write(sound.Loop);
                writer.Write(sound.Volume);
                writer.Write(sound.Spatial);
                writer.Write(sound.AllowHrtf);
                writer.Write(sound.FadeInSeconds);
                writer.Write(sound.FadeOutSeconds);
                writer.Write(sound.CrossfadeSeconds.HasValue);
                if (sound.CrossfadeSeconds.HasValue)
                    writer.Write(sound.CrossfadeSeconds.Value);
                writer.Write(sound.Pitch);
                writer.Write(sound.Pan);
                WriteNullableSingle(writer, sound.MinDistance);
                WriteNullableSingle(writer, sound.MaxDistance);
                WriteNullableSingle(writer, sound.Rolloff);
                writer.Write(sound.Global);
                writer.Write(sound.StartAreaId ?? string.Empty);
                writer.Write(sound.EndAreaId ?? string.Empty);
                WriteNullableVector3(writer, sound.StartPosition);
                WriteNullableSingle(writer, sound.StartRadiusMeters);
                WriteNullableVector3(writer, sound.EndPosition);
                WriteNullableSingle(writer, sound.EndRadiusMeters);
                WriteNullableVector3(writer, sound.Position);
                WriteNullableSingle(writer, sound.SpeedMetersPerSecond);
            }
        }

        private static IReadOnlyDictionary<string, TopSpeed.Data.TrackSoundSourceDefinition> ReadSoundDefinitions(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count < 0)
                throw new InvalidDataException("Invalid sound definition count.");

            var map = new Dictionary<string, TopSpeed.Data.TrackSoundSourceDefinition>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadString();
                var type = (TopSpeed.Data.TrackSoundSourceType)reader.ReadByte();
                var path = reader.ReadString();

                var variantPathCount = reader.ReadInt32();
                if (variantPathCount < 0)
                    throw new InvalidDataException("Invalid variant path count.");
                var variantPaths = new string[variantPathCount];
                for (var variantIndex = 0; variantIndex < variantPathCount; variantIndex++)
                    variantPaths[variantIndex] = reader.ReadString();

                var variantSourceCount = reader.ReadInt32();
                if (variantSourceCount < 0)
                    throw new InvalidDataException("Invalid variant source count.");
                var variantSourceIds = new string[variantSourceCount];
                for (var sourceIndex = 0; sourceIndex < variantSourceCount; sourceIndex++)
                    variantSourceIds[sourceIndex] = reader.ReadString();

                var randomMode = (TopSpeed.Data.TrackSoundRandomMode)reader.ReadByte();
                var loop = reader.ReadBoolean();
                var volume = reader.ReadSingle();
                var spatial = reader.ReadBoolean();
                var allowHrtf = reader.ReadBoolean();
                var fadeInSeconds = reader.ReadSingle();
                var fadeOutSeconds = reader.ReadSingle();
                var hasCrossfade = reader.ReadBoolean();
                float? crossfadeSeconds = null;
                if (hasCrossfade)
                    crossfadeSeconds = reader.ReadSingle();
                var pitch = reader.ReadSingle();
                var pan = reader.ReadSingle();
                var minDistance = ReadNullableSingle(reader);
                var maxDistance = ReadNullableSingle(reader);
                var rolloff = ReadNullableSingle(reader);
                var global = reader.ReadBoolean();
                var startAreaId = NormalizeNull(reader.ReadString());
                var endAreaId = NormalizeNull(reader.ReadString());
                var startPosition = ReadNullableVector3(reader);
                var startRadius = ReadNullableSingle(reader);
                var endPosition = ReadNullableVector3(reader);
                var endRadius = ReadNullableSingle(reader);
                var position = ReadNullableVector3(reader);
                var speedMetersPerSecond = ReadNullableSingle(reader);

                map[id] = new TopSpeed.Data.TrackSoundSourceDefinition(
                    id,
                    type,
                    string.IsNullOrWhiteSpace(path) ? null : path,
                    variantPaths,
                    variantSourceIds,
                    randomMode,
                    loop,
                    volume,
                    spatial,
                    allowHrtf,
                    fadeInSeconds,
                    fadeOutSeconds,
                    crossfadeSeconds,
                    pitch,
                    pan,
                    minDistance,
                    maxDistance,
                    rolloff,
                    global,
                    startAreaId,
                    endAreaId,
                    startPosition,
                    startRadius,
                    endPosition,
                    endRadius,
                    position,
                    speedMetersPerSecond);
            }

            return map;
        }
    }
}
