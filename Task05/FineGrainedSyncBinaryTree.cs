using System;
using System.Collections.Generic;
using System.Threading;

namespace Task05
{
    public class FineGrainedSyncBinaryTree<T> : BinaryTree<T> where T : struct, IComparable<T>
    {
        private SafeNode<T>? _root;
        private readonly Mutex _mtxRoot = new Mutex();
        
        public FineGrainedSyncBinaryTree(IEnumerable<T> collection) : base(collection) {}
        
        public override T? Find(T targetValue)
        {
            SafeNode<T> current = _root;
            current?.Mtx.WaitOne();
            if (current?.Value.CompareTo(targetValue) == 0)
            {
                current.Mtx.ReleaseMutex();
                return targetValue;
            }

            while (current != null)
            {
                if (current.Value.CompareTo(targetValue) < 0)
                {
                    current.Right?.Mtx.WaitOne();
                    current.Mtx.ReleaseMutex();
                    current = current.Right;
                }
                else if (current.Value.CompareTo(targetValue) > 0)
                {
                    current.Left?.Mtx.WaitOne();
                    current.Mtx.ReleaseMutex();
                    current = current.Left;
                }
                else
                {
                    current.Mtx.ReleaseMutex();
                    return targetValue;
                }
            }
            
            return null;
        }
        
        public override void Insert(T value)
        {
            SafeNode<T> current = _root;
            SafeNode<T> newNode = new SafeNode<T>(value);
            _mtxRoot.WaitOne();
            if (_root == null)
            {
                _root = newNode;
                _mtxRoot.ReleaseMutex();
                return;
            }
            current?.Mtx.WaitOne();
            
            while (true)
            {
                if (current?.Value.CompareTo(newNode.Value) < 0)
                {
                    if (current.Right == null)
                    {
                        current.Right = newNode;
                        current.Mtx.ReleaseMutex();
                        break;
                    }
                    current.Right?.Mtx.WaitOne();
                    current.Mtx.ReleaseMutex();
                    current = current.Right;
                }
                else if (current?.Value.CompareTo(newNode.Value) > 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = newNode;
                        current.Mtx.ReleaseMutex();
                        break;
                    }
                    current.Left?.Mtx.WaitOne();
                    current.Mtx.ReleaseMutex();
                    current = current.Left;
                }
                else
                {
                    current?.Mtx.ReleaseMutex();
                    break;
                }
            }
        }

        public override void Remove(T targetValue)
        {
            // Find target node to remove
            SafeNode<T> current = _root;
            SafeNode<T> target = null;
            SafeNode<T> parentOfTarget = null;
            current?.Mtx.WaitOne();
            if (current?.Value.CompareTo(targetValue) == 0)
            {
                target = current;
            }
            else
            {
                while (current != null)
                {
                    if (current.Value.CompareTo(targetValue) < 0)
                    {
                        current.Right?.Mtx.WaitOne();
                        current.Mtx.ReleaseMutex();
                        parentOfTarget = current;
                        current = current.Right;
                    }
                    else if (current.Value.CompareTo(targetValue) > 0)
                    {
                        current.Left?.Mtx.WaitOne();
                        current.Mtx.ReleaseMutex();
                        parentOfTarget = current;
                        current = current.Left;
                    }
                    else
                    {
                        target = current;
                        break;
                    }
                }
            }

            if (target == null) return;
            
            // Find nearest neighbors from left and right subtrees
            SafeNode<T> leftNeighbor = target.Left;
            SafeNode<T> prevOfLeftNeighbor = target;
            leftNeighbor?.Mtx.WaitOne();
            SafeNode<T> rightNeighbor = target.Right;
            SafeNode<T> prevOfRightNeighbor = target;
            rightNeighbor?.Mtx.WaitOne();
            while (leftNeighbor?.Right != null || rightNeighbor?.Left != null)
            {
                leftNeighbor?.Right?.Mtx.WaitOne();
                if (leftNeighbor?.Right != null)
                {
                    leftNeighbor?.Mtx.ReleaseMutex();
                    prevOfLeftNeighbor = leftNeighbor;
                    leftNeighbor = leftNeighbor?.Right;
                }

                rightNeighbor?.Left?.Mtx.WaitOne();
                if (rightNeighbor?.Left != null)
                {
                    rightNeighbor?.Mtx.ReleaseMutex();
                    prevOfRightNeighbor = rightNeighbor;
                    rightNeighbor = rightNeighbor?.Left;
                }
            }
            
            // Target node is leaf, just delete it
            if (leftNeighbor == null && rightNeighbor == null)
            {
                if (parentOfTarget?.Left?.Equals(target) == true)
                {
                    parentOfTarget.Left = null;
                }
                else if (parentOfTarget?.Right?.Equals(target) == true)
                {
                    parentOfTarget.Right = null;
                }
                target.Mtx.ReleaseMutex();
                return;
            }
            
            // Otherwise, swap values with one of nearest neighbors in subtrees
            if (leftNeighbor != null)
            {
                target.Value = leftNeighbor.Value;
                if (prevOfLeftNeighbor.Equals(target))
                {
                    target.Left = leftNeighbor.Left;
                }
                else
                {
                    prevOfLeftNeighbor.Right = leftNeighbor.Left;
                }
            }
            else
            {
                target.Value = rightNeighbor.Value;
                if (prevOfRightNeighbor.Equals(target))
                {
                    target.Right = rightNeighbor.Right;
                }
                else
                {
                    prevOfRightNeighbor.Left = rightNeighbor.Right;
                }
            }
            leftNeighbor?.Mtx.ReleaseMutex();
            rightNeighbor?.Mtx.ReleaseMutex();
            target.Mtx.ReleaseMutex();
        }
        
        public override void Print()
        {
            Traverse(_root);
            Console.WriteLine();
        }

        private void Traverse(SafeNode<T> current)
        {
            if (current != null)
            {
                Traverse(current.Left);
                Console.Write(current.Value + " ");
                Traverse(current.Right);
            }
        }
    }
}