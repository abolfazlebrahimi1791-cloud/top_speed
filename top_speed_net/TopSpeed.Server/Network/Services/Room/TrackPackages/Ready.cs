using TopSpeed.Protocol;

namespace TopSpeed.Server.Network
{
    internal sealed partial class RaceServer
    {
        private sealed partial class Room
        {
            public void HandleReady(PlayerConnection player, PacketTrackPackageReady packet)
            {
                if (!player.RoomId.HasValue)
                    return;
                if (!_owner._rooms.TryGetValue(player.RoomId.Value, out var room))
                    return;

                var hash = TrackPackageRef.NormalizeHash(packet.Hash);
                if (!room.TrackSelection.IsCustomPackage)
                    return;
                if (!string.Equals(room.TrackSelection.Hash, hash, System.StringComparison.OrdinalIgnoreCase))
                    return;

                _owner.MarkPlayerTrackReady(room, player.Id);
                if (room.PreparingRace)
                    _owner._race.TryStartAfterLoadout(room);
            }
        }
    }
}

