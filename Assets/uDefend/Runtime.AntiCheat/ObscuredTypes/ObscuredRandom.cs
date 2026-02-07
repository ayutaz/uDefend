using System;
using System.Security.Cryptography;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Thread-safe random number generator for ObscuredTypes key generation.
    /// Uses CSPRNG (RandomNumberGenerator) to prevent key prediction attacks.
    /// </summary>
    internal static class ObscuredRandom
    {
        /// <summary>
        /// Returns a random non-zero int, safe to call from any thread.
        /// </summary>
        public static int Next()
        {
            Span<byte> buf = stackalloc byte[4];
            RandomNumberGenerator.Fill(buf);
            int key = BitConverter.ToInt32(buf) | 1; // ensure non-zero by setting LSB
            return key;
        }

        /// <summary>
        /// Returns a random non-zero long, safe to call from any thread.
        /// </summary>
        public static long NextLong()
        {
            Span<byte> buf = stackalloc byte[8];
            RandomNumberGenerator.Fill(buf);
            long key = BitConverter.ToInt64(buf) | 1L; // ensure non-zero by setting LSB
            return key;
        }
    }
}
