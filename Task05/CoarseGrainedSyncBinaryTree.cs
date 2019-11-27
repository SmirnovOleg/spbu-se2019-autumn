using System;
using System.Collections.Generic;
using System.Threading;

namespace Task05
{
    public class CoarseGrainedSyncBinaryTree<T> : BinaryTree<T> where T : struct, IComparable<T>
    {
        private readonly Mutex _mutex = new Mutex();

        public CoarseGrainedSyncBinaryTree(IEnumerable<T> collection) : base(collection) {}

        public override void Insert(T value)
        {
            _mutex.WaitOne();
            base.Insert(value);
            _mutex.ReleaseMutex();
        }

        public override T? Find(T targetValue)
        {
            _mutex.WaitOne();
            var result = base.Find(targetValue);
            _mutex.ReleaseMutex();
            return result;
        }

        public override void Remove(T targetValue)
        {
            _mutex.WaitOne();
            base.Remove(targetValue);
            _mutex.ReleaseMutex();
        }
    }
}