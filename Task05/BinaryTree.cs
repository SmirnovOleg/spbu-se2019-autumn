using System;
using System.Collections.Generic;

namespace Task05
{
    public class BinaryTree<T> where T : struct, IComparable<T>
    {
        private Node<T> _root;

        public BinaryTree()
        {
            _root = null;
        }
        
        public BinaryTree(IEnumerable<T> collection)
        {
            foreach (var value in collection)
            {
                Insert(value);
            }
        }

        public virtual void Insert(T value)
        {
            Node<T> newNode = new Node<T>(value);
            if (_root == null)
            {
                _root = newNode;
                return;
            }

            Node<T> current = _root;
            while (current.Value.CompareTo(newNode.Value) != 0)
            {
                if (current.Value.CompareTo(newNode.Value) < 0)
                {
                    if (current.Right == null)
                    {
                        current.Right = newNode;
                        break;
                    }
                    current = current.Right;
                }
                else
                {
                    if (current.Left == null)
                    {
                        current.Left = newNode;
                        break;
                    }
                    current = current.Left;
                }
            }
        }

        public virtual T? Find(T targetValue)
        {
            Node<T> current = _root;
            while (current != null && current.Value.CompareTo(targetValue) != 0)
            {
                current = current.Value.CompareTo(targetValue) < 0 ? current.Right : current.Left;
            }

            return current?.Value;
        }

        public virtual void Remove(T targetValue)
        {
            // Find necessary node with targetValue and its parent
            Node<T> parent = null;
            Node<T> current = _root;
            while (current != null && current.Value.CompareTo(targetValue) != 0)
            {
                parent = current;
                if (current.Left?.Value.CompareTo(targetValue) == 0)
                {
                    current = current.Left;
                    break;
                }

                if (current.Right?.Value.CompareTo(targetValue) == 0)
                {
                    current = current.Right;
                    break;
                }

                current = current.Value.CompareTo(targetValue) < 0 ? current.Right : current.Left;
            }

            if (current == null) return;
            Node<T> target = current;
            if (target.Left != null)
            {
                // Find node with max value in left subtree and swap its value with target
                current = target.Left;
                Node<T> prev = target;
                while (current?.Right != null)
                {
                    prev = current;
                    current = current.Right;
                }
                
                target.Value = current.Value;
                if (prev == target)
                {
                    target.Left = current.Left;
                }
                else
                {
                    prev.Right = current.Left;
                }
            }
            else if (target.Right != null)
            {
                // Otherwise, find node with min value in right subtree and swap its value with target
                current = target.Right;
                Node<T> prev = target;
                while (current?.Left != null)
                {
                    prev = current;
                    current = current.Left;
                }
                
                target.Value = current.Value;
                if (prev == target)
                {
                    target.Right = current.Right;
                }
                else
                {
                    prev.Left = current.Right;
                }
            }
            else
            {
                // In case if target node has no children, update its parent
                if (parent == null)
                {
                    _root = null;
                }
                else if (parent.Left != null && parent.Left.Equals(current))
                {
                    parent.Left = null;
                }
                else
                {
                    parent.Right = null;
                }
            }
            
        }

        public void Print()
        {
            Traverse(_root);
            Console.WriteLine();
        }

        private void Traverse(Node<T> current)
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