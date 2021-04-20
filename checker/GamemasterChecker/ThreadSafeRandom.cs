namespace GamemasterChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    // code used from https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
    public static class ThreadSafeRandom
    {
        private static readonly RNGCryptoServiceProvider Global = new();
        [ThreadStatic]
        private static Random? local;

        public static void NextBytes(byte[] array)
        {
            Random? inst = local;
            if (inst == null)
            {
                Span<byte> buffer = stackalloc byte[4];
                Global.GetBytes(buffer);
                local = inst = new Random(
                    BitConverter.ToInt32(buffer));
            }

            inst.NextBytes(array);
        }

        public static int Next()
        {
            Random? inst = local;
            if (inst == null)
            {
                Span<byte> buffer = stackalloc byte[4];
                Global.GetBytes(buffer);
                local = inst = new Random(
                    BitConverter.ToInt32(buffer));
            }

            return inst.Next();
        }

        public static int Next(int n)
        {
            return Next() % n;
        }
    }
}
