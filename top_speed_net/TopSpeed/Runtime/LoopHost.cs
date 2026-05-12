using System;
using System.Diagnostics;
using System.Threading;

namespace TopSpeed.Runtime
{
    internal sealed class LoopHost : ILoopHost
    {
        // 8 ms (125 fps) race tick needs the Windows scheduler at its 1 ms
        // resolution; any longer interval is fine on the default 15 ms tick and
        // releasing high-resolution mode there is what stops the kernel timer
        // from waking the CPU 1000 times a second while the game is idle.
        private const int HighResolutionIntervalThresholdMs = 8;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Thread? _thread;
        private volatile bool _running;
        private long _lastTicks;
        private bool _highResolutionRequested;
        private Action<float>? _onTick;
        private Func<int>? _resolveIntervalMs;

        public bool IsRunning => _running;

        public void Start(Action<float> onTick, Func<int> resolveIntervalMs)
        {
            if (onTick == null)
                throw new ArgumentNullException(nameof(onTick));
            if (resolveIntervalMs == null)
                throw new ArgumentNullException(nameof(resolveIntervalMs));
            if (_thread != null)
                return;

            _onTick = onTick;
            _resolveIntervalMs = resolveIntervalMs;
            _running = true;
            _lastTicks = 0L;
            _stopwatch.Restart();

            _thread = new Thread(RunLoop)
            {
                IsBackground = true,
                Name = "GameLoop"
            };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            if (_thread == null)
                return;
            if (_thread.IsAlive)
                _thread.Join(200);
            _thread = null;
            _stopwatch.Stop();
            _onTick = null;
            _resolveIntervalMs = null;
            ReleaseHighResolutionIfNeeded();
        }

        public void Dispose()
        {
            Stop();
        }

        private void RunLoop()
        {
            while (_running)
            {
                var onTick = _onTick;
                var resolveIntervalMs = _resolveIntervalMs;
                if (onTick == null || resolveIntervalMs == null)
                    break;

                var now = _stopwatch.ElapsedTicks;
                if (_lastTicks == 0L)
                    _lastTicks = now;
                var deltaSeconds = (float)(now - _lastTicks) / Stopwatch.Frequency;
                _lastTicks = now;
                onTick(deltaSeconds);

                var intervalMs = resolveIntervalMs();
                if (intervalMs <= 0)
                    intervalMs = 8;
                ApplyHighResolutionForInterval(intervalMs);
                Thread.Sleep(intervalMs);
            }

            ReleaseHighResolutionIfNeeded();
        }

        private void ApplyHighResolutionForInterval(int intervalMs)
        {
            var shouldRequest = intervalMs <= HighResolutionIntervalThresholdMs;
            if (shouldRequest == _highResolutionRequested)
                return;

            if (shouldRequest)
            {
                WindowsTimerResolution.RequestHighResolution();
                _highResolutionRequested = true;
            }
            else
            {
                WindowsTimerResolution.ReleaseHighResolution();
                _highResolutionRequested = false;
            }
        }

        private void ReleaseHighResolutionIfNeeded()
        {
            if (!_highResolutionRequested)
                return;

            WindowsTimerResolution.ReleaseHighResolution();
            _highResolutionRequested = false;
        }
    }
}



