using System.IO;
using TopSpeed.Audio;
using TopSpeed.Core;
using TopSpeed.Input;
using TS.Audio;

namespace TopSpeed.Drive.Multiplayer
{
    internal sealed partial class MultiplayerSession
    {
        private SoundAsset? _micOpenSound;
        private SoundAsset? _micCloseSound;

        private void OnLocalVoiceTransmissionStateChanged(bool opened)
        {
            PlayMicToggleCue(opened);
        }

        private void PlayMicToggleCue(bool opened)
        {
            var sound = opened ? GetMicOpenSound() : GetMicCloseSound();
            if (sound == null)
                return;

            try
            {
                _audio.PlayOneShot(sound, AudioEngineOptions.UiBusName, configure: handle =>
                {
                    handle.SetVolumePercent(_settings, AudioVolumeCategory.OnlineServerEvents, 100);
                });
            }
            catch
            {
            }
        }

        private SoundAsset? GetMicOpenSound()
        {
            return GetNetworkSound(ref _micOpenSound, Path.Combine("network", "comm", "mic_open.wav"));
        }

        private SoundAsset? GetMicCloseSound()
        {
            var sound = GetNetworkSound(ref _micCloseSound, Path.Combine("network", "mic_close.wav"));
            if (sound != null)
                return sound;

            return GetNetworkSound(ref _micCloseSound, Path.Combine("network", "comm", "mic_close.wav"));
        }

        private SoundAsset? GetNetworkSound(ref SoundAsset? cache, string relativePath)
        {
            if (cache != null)
                return cache;

            var path = Path.Combine(AssetPaths.SoundsRoot, relativePath);
            if (!_audio.TryResolvePath(path, out var fullPath))
                return null;

            try
            {
                cache = _audio.LoadAsset(fullPath, streamFromDisk: false);
                return cache;
            }
            catch
            {
                return null;
            }
        }
    }
}
