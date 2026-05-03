using TopSpeed.Protocol;

namespace TopSpeed.Server.Network
{
    internal sealed partial class RaceServer
    {
        private sealed partial class Room
        {
            private bool IsSelectionEnabled(RaceRoom room)
            {
                return room != null
                    && _owner._config.Features.CustomTracks
                    && (room.GameRulesFlags & (uint)RoomGameRules.CustomTracks) != 0u;
            }
        }
    }
}

