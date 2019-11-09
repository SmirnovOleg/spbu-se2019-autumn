using System;
using System.Collections.Generic;
using System.Threading;

namespace Task03
{
    public static class SharedData<T>
    {
        public static readonly List<T> Buffer = new List<T>();
        
        internal static readonly SemaphoreSlim NonEmptySem = new SemaphoreSlim(0, Int32.MaxValue);
        internal static readonly Mutex ProducingMtx = new Mutex();
        internal static readonly Mutex ConsumingMtx = new Mutex();

        internal static readonly Random RandomGenerator = new Random();
    }
}