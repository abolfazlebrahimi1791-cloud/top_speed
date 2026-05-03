using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TopSpeed.Protocol
{
    public static partial class TrackPackageCodec
    {
        private static void WriteWeatherProfiles(BinaryWriter writer, IReadOnlyDictionary<string, TopSpeed.Data.TrackWeatherProfile> profiles)
        {
            var ordered = profiles.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
            writer.Write(ordered.Length);
            for (var i = 0; i < ordered.Length; i++)
            {
                var profile = ordered[i].Value;
                writer.Write(profile.Id ?? string.Empty);
                writer.Write((byte)profile.Kind);
                writer.Write(profile.LongitudinalWindMps);
                writer.Write(profile.LateralWindMps);
                writer.Write(profile.AirDensityKgPerM3);
                writer.Write(profile.DraftingFactor);
                writer.Write(profile.TemperatureC);
                writer.Write(profile.Humidity);
                writer.Write(profile.PressureKpa);
                writer.Write(profile.VisibilityM);
                writer.Write(profile.RainGain);
                writer.Write(profile.WindGain);
                writer.Write(profile.StormGain);
            }
        }

        private static IReadOnlyDictionary<string, TopSpeed.Data.TrackWeatherProfile> ReadWeatherProfiles(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count < 0)
                throw new InvalidDataException("Invalid weather profile count.");

            var map = new Dictionary<string, TopSpeed.Data.TrackWeatherProfile>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadString();
                map[id] = new TopSpeed.Data.TrackWeatherProfile(
                    id,
                    (TopSpeed.Data.TrackWeather)reader.ReadByte(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
            }

            return map;
        }
    }
}
