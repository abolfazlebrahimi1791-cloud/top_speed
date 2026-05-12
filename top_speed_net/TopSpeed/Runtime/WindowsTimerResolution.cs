#if WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TopSpeed.Runtime
{
    // Windows system-timer resolution is global to the process and affects every
    // Thread.Sleep / Wait the app issues. Holding 1 ms tick continuously also
    // forces the Windows kernel scheduler into a 1 ms heartbeat for as long as
    // the process is alive, which shows up as elevated idle CPU. The game only
    // really needs 1 ms precision during a race (so 8 ms / 125 fps ticks are
    // honored), not while sitting in a menu. This wrapper lets the game request
    // and release high resolution dynamically without losing the request from
    // any single caller.
    internal static class WindowsTimerResolution
    {
        private const uint HighResolutionMilliseconds = 1;
        private static int _refCount;
        private static bool _activeHighResolution;

        public static void RequestHighResolution()
        {
            if (Interlocked.Increment(ref _refCount) != 1)
                return;

            try
            {
                if (timeBeginPeriod(HighResolutionMilliseconds) == 0)
                    _activeHighResolution = true;
            }
            catch
            {
                _activeHighResolution = false;
            }
        }

        public static void ReleaseHighResolution()
        {
            var remaining = Interlocked.Decrement(ref _refCount);
            if (remaining < 0)
            {
                Interlocked.Exchange(ref _refCount, 0);
                return;
            }
            if (remaining != 0)
                return;

            if (!_activeHighResolution)
                return;

            try
            {
                timeEndPeriod(HighResolutionMilliseconds);
            }
            catch
            {
            }
            _activeHighResolution = false;
        }

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        private static extern uint timeEndPeriod(uint uPeriod);
    }
}
#else
namespace TopSpeed.Runtime
{
    // Non-Windows platforms do not have a global timer-resolution knob; the
    // requests are no-ops so callers can use the same API everywhere.
    internal static class WindowsTimerResolution
    {
        public static void RequestHighResolution()
        {
        }

        public static void ReleaseHighResolution()
        {
        }
    }
}
#endif
