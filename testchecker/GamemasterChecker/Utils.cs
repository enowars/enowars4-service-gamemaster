using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GamemasterChecker
{
    public class Utils
    {
        // THis is a threadsafe random, see https://stackoverflow.com/questions/19270507/correct-way-to-use-random-in-multithread-application
        static int seed = Environment.TickCount;
        public static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
    }
}
