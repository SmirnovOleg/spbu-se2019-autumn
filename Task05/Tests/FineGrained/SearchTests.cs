using System.Collections.Generic;
using NUnit.Framework;
using Task05;

namespace Tests.FineGrained
{
    [TestFixture]
    public class SearchTests
    {
        [TestCase(0)]
        public void Find_ValueInEmptyFineTree_ReturnNull(int value)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            
            var actual = tree.Find(value);
            
            Assert.IsNull(actual);
        }
        
        [TestCase(new [] {5}, 5)]
        [TestCase(new [] {5, 9, 7, 8}, 8)]
        [TestCase(new [] {5, 9, 1, 7, 6, 8}, 9)]
        [TestCase(new [] {1, 2, 3, 4, 5}, 5)]
        public void Find_ValueInFineGrainedTree_ReturnValue(IEnumerable<int> elements, int value)
        {
            var tree = new FineGrainedSyncBinaryTree<int>(elements);
            
            var actual = tree.Find(value);
            
            Assert.AreEqual(value, actual);
        }
        
        [TestCase(new[] {5, 9, 1, 7, 6, 8}, 100)]
        public void Find_NonExistentValueInFineTree_ReturnNull(IEnumerable<int> elements, int value)
        {
            var tree = new FineGrainedSyncBinaryTree<int>(elements);
            
            var actual = tree.Find(value);

            Assert.IsNull(actual);
        }
    }
}