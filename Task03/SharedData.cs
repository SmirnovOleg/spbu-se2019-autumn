using System;
using System.Collections.Generic;
using System.Threading;

namespace Task03
{
    public static class SharedData<T>
    {
        public static readonly List<T> Buffer = new List<T>();
        
        internal static readonly SemaphoreSlim WritingSem = new SemaphoreSlim(1, 1);
        internal static readonly Mutex ReadingHelperMtx = new Mutex();
        internal static readonly Mutex WaitingForWorkMtx = new Mutex();

        internal static int NowReading = 0;
        internal static readonly Random RandomGenerator = new Random();
    }
}