using System.Text.Json.Serialization;

namespace TopSpeed.Server.Config
{
    [JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(ServerModerationSettings))]
    [JsonSerializable(typeof(ServerSettings))]
    internal partial class ServerSettingsJsonContext : JsonSerializerContext
    {
    }
}
