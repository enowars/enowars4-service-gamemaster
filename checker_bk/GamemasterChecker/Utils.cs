using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class Utils
    {
        // This is a threadsafe random, see https://stackoverflow.com/questions/19270507/correct-way-to-use-random-in-multithread-application
        private static int seed = Environment.TickCount;
        private static readonly ThreadLocal<Random> _Random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
#pragma warning disable CS8603
        public static Random Random => _Random.Value;
#pragma warning restore CS8603
    }
}
