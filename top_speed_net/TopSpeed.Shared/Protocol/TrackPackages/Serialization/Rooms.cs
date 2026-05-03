using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        private static void WriteRoomProfiles(BinaryWriter writer, IReadOnlyDictionary<string, TopSpeed.Data.TrackRoomDefinition> rooms)
        {
            var ordered = rooms.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
            writer.Write(ordered.Length);
            for (var i = 0; i < ordered.Length; i++)
            {
                var room = ordered[i].Value;
                writer.Write(ordered[i].Key ?? string.Empty);
                writer.Write(room.Name ?? string.Empty);
                writer.Write(room.ReverbTimeSeconds);
                writer.Write(room.ReverbGain);
                writer.Write(room.HfDecayRatio);
                writer.Write(room.LateReverbGain);
                writer.Write(room.Diffusion);
                writer.Write(room.AirAbsorption);
                writer.Write(room.OcclusionScale);
                writer.Write(room.TransmissionScale);
                WriteNullableSingle(writer, room.OcclusionOverride);
                WriteNullableSingle(writer, room.TransmissionOverrideLow);
                WriteNullableSingle(writer, room.TransmissionOverrideMid);
                WriteNullableSingle(writer, room.TransmissionOverrideHigh);
                WriteNullableSingle(writer, room.AirAbsorptionOverrideLow);
                WriteNullableSingle(writer, room.AirAbsorptionOverrideMid);
                WriteNullableSingle(writer, room.AirAbsorptionOverrideHigh);
            }
        }

        private static IReadOnlyDictionary<string, TopSpeed.Data.TrackRoomDefinition> ReadRoomProfiles(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count < 0)
                throw new InvalidDataException("Invalid room profile count.");

            var map = new Dictionary<string, TopSpeed.Data.TrackRoomDefinition>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadString();
                if (string.IsNullOrWhiteSpace(id))
                    throw new InvalidDataException("Invalid room profile id.");
                map[id] = new TopSpeed.Data.TrackRoomDefinition(
                    id,
                    NormalizeNull(reader.ReadString()),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    ReadNullableSingle(reader),
                    ReadNullableSingle(reader),
                    ReadNullableSingle(reader),
                    ReadNullableSingle(reader),
                    ReadNullableSingle(reader),
                    ReadNullableSingle(reader),
                    ReadNullableSingle(reader));
            }

            return map;
        }

        private static void WriteRoomOverrides(BinaryWriter writer, TopSpeed.Data.TrackRoomOverrides? overrides)
        {
            writer.Write(overrides != null);
            if (overrides == null)
                return;

            WriteNullableSingle(writer, overrides.ReverbTimeSeconds);
            WriteNullableSingle(writer, overrides.ReverbGain);
            WriteNullableSingle(writer, overrides.HfDecayRatio);
            WriteNullableSingle(writer, overrides.LateReverbGain);
            WriteNullableSingle(writer, overrides.Diffusion);
            WriteNullableSingle(writer, overrides.AirAbsorption);
            WriteNullableSingle(writer, overrides.OcclusionScale);
            WriteNullableSingle(writer, overrides.TransmissionScale);
            WriteNullableSingle(writer, overrides.OcclusionOverride);
            WriteNullableSingle(writer, overrides.TransmissionOverrideLow);
            WriteNullableSingle(writer, overrides.TransmissionOverrideMid);
            WriteNullableSingle(writer, overrides.TransmissionOverrideHigh);
            WriteNullableSingle(writer, overrides.AirAbsorptionOverrideLow);
            WriteNullableSingle(writer, overrides.AirAbsorptionOverrideMid);
            WriteNullableSingle(writer, overrides.AirAbsorptionOverrideHigh);
        }

        private static TopSpeed.Data.TrackRoomOverrides? ReadRoomOverrides(BinaryReader reader)
        {
            if (!reader.ReadBoolean())
                return null;

            var overrides = new TopSpeed.Data.TrackRoomOverrides
            {
                ReverbTimeSeconds = ReadNullableSingle(reader),
                ReverbGain = ReadNullableSingle(reader),
                HfDecayRatio = ReadNullableSingle(reader),
                LateReverbGain = ReadNullableSingle(reader),
                Diffusion = ReadNullableSingle(reader),
                AirAbsorption = ReadNullableSingle(reader),
                OcclusionScale = ReadNullableSingle(reader),
                TransmissionScale = ReadNullableSingle(reader),
                OcclusionOverride = ReadNullableSingle(reader),
                TransmissionOverrideLow = ReadNullableSingle(reader),
                TransmissionOverrideMid = ReadNullableSingle(reader),
                TransmissionOverrideHigh = ReadNullableSingle(reader),
                AirAbsorptionOverrideLow = ReadNullableSingle(reader),
                AirAbsorptionOverrideMid = ReadNullableSingle(reader),
                AirAbsorptionOverrideHigh = ReadNullableSingle(reader)
            };
            return overrides.HasAny ? overrides : null;
        }
    }
}
