using System;
using System.Threading;

namespace Task05
{
    internal class SafeNode<T> : Node<T> where T : IComparable<T>
    {
        public Mutex Mtx;
        
        public SafeNode(T value) : base(value)
        {
            Mtx = new Mutex();
        }
    }
}