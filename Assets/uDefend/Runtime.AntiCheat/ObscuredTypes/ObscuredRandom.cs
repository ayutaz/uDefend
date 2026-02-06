using System;
using System.Threading;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Thread-safe random number generator for ObscuredTypes key generation.
    /// Uses per-thread System.Random instances for performance and thread safety.
    /// This is intentionally NOT cryptographically secure - ObscuredTypes use
    /// obfuscation (not encryption) for memory protection against casual cheating tools.
    /// </summary>
    internal static class ObscuredRandom
    {
        [ThreadStatic] private static Random s_random;

        /// <summary>
        /// Returns a random non-zero int, safe to call from any thread.
        /// </summary>
        public static int Next()
        {
            if (s_random == null)
                s_random = new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId);

            int key = s_random.Next(1, int.MaxValue);
            return key;
        }

        /// <summary>
        /// Returns a random non-zero long, safe to call from any thread.
        /// </summary>
        public static long NextLong()
        {
            if (s_random == null)
                s_random = new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId);

            long high = (long)s_random.Next(1, int.MaxValue) << 32;
            long low = (long)s_random.Next(0, int.MaxValue);
            return high | low;
        }
    }
}
