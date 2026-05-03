using TopSpeed.Protocol;

namespace TopSpeed.Server.Network
{
    internal sealed partial class RaceServer
    {
        private sealed partial class Room
        {
            public void HandleCatalogRequest(PlayerConnection player, PacketTrackPackageCatalogRequest packet)
            {
                if (!TryGetHosted(player, out var room))
                    return;

                if (!_owner._config.Features.CustomTracks || !IsSelectionEnabled(room))
                {
                    _owner.SendTrackPackageCatalog(player, new PacketTrackPackageCatalog());
                    return;
                }

                _owner.SendTrackPackageCatalog(player, _owner.BuildTrackPackageCatalog());
            }
        }
    }
}

