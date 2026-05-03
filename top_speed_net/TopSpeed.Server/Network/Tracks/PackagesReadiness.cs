using System.Collections.Generic;
using TopSpeed.Protocol;

namespace TopSpeed.Server.Network
{
    internal sealed partial class RaceServer
    {
        private bool EnsureRoomTrackPackageReady(RaceRoom room, IEnumerable<uint> participantIds)
        {
            if (room == null || room.TrackSelection == null || !room.TrackSelection.IsCustomPackage)
                return true;

            if (!TryGetTrackPackage(room.TrackSelection.Hash, out var package))
                return false;

            var allReady = true;
            foreach (var id in participantIds)
            {
                if (room.TrackReadyPlayers.Contains(id))
                    continue;

                allReady = false;
                if (_players.TryGetValue(id, out var player))
                    SendTrackPackageToPlayer(player, package);
            }

            return allReady;
        }

        private void ResetRoomTrackReadiness(RaceRoom room)
        {
            room.TrackReadyPlayers.Clear();
        }

        private void MarkPlayerTrackReady(RaceRoom room, uint playerId)
        {
            room.TrackReadyPlayers.Add(playerId);
        }
    }
}
