using System;
using System.Threading;

namespace Task05
{
    internal class SafeNode<T> : Node<T> where T : IComparable<T>
    {
        public readonly Mutex Mtx;
        public new SafeNode<T> Left;
        public new SafeNode<T> Right;

        public SafeNode(T value) : base(value)
        {
            Mtx = new Mutex();
            Left = null;
            Right = null;
        }
    }
}