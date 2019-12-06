using System;
using System.Collections.Generic;
using Task05;

namespace Tests
{
    internal static class Utils
    {
        public static bool ContentEquals<T>(BinaryTree<T> first, BinaryTree<T> second) where T : struct, IComparable<T>
        {
            var elementsOfFirstTree = new HashSet<T>();
            first.ForEach(node => { elementsOfFirstTree.Add(node.Value); });
            var elementsOfSecondTree = new HashSet<T>();
            second.ForEach(node => { elementsOfSecondTree.Add(node.Value); });
            return elementsOfFirstTree.SetEquals(elementsOfSecondTree);
        }

        public static bool ValidateStructure<T>(BinaryTree<T> tree) where T : struct, IComparable<T>
        {
            return tree.All(node => 
                node.Left?.Value.CompareTo(node.Value) < 0
                && node.Right?.Value.CompareTo(node.Value) > 0);
        }
    }
}