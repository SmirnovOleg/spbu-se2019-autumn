using NUnit.Framework;
using Task05;

namespace Tests.CoarseGrained
{
    [TestFixture]
    public class InsertionTests
    {
        [TestCase(0)]
        public void Insert_RootValueToEmptyCoarseTree_UpdateTree(int value)
        {
            var tree = new CoarseGrainedSyncBinaryTree<int>();
            
            tree.Insert(value);
            
            Assert.AreEqual(value, tree.Root.Value);
            Assert.AreEqual(null, tree.Root.Left);
            Assert.AreEqual(null, tree.Root.Right);
        }
        
        [TestCase(0)]
        public void Insert_ExistentValueToCoarseTree_ReturnSameTree(int value)
        {
            var tree = new CoarseGrainedSyncBinaryTree<int>();

            tree.Insert(value);
            tree.Insert(value);
            
            Assert.AreEqual(value, tree.Root.Value);
            Assert.AreEqual(null, tree.Root.Left);
            Assert.AreEqual(null, tree.Root.Right);
        }
        
        [TestCase(5, 3, 1)]
        public void Insert_InOrderLeftLeftToCoarseTree_UpdateTree(int rootValue, int leftValue, int leftOfLeftValue)
        {
            var tree = new CoarseGrainedSyncBinaryTree<int>();
            
            tree.Insert(rootValue);
            tree.Insert(leftValue);
            tree.Insert(leftOfLeftValue);
            
            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Right);
            Assert.IsNotNull(tree.Root.Left);
            
            Assert.AreEqual(leftValue, tree.Root.Left.Value);
            Assert.IsNull(tree.Root.Left.Right);
            Assert.IsNotNull(tree.Root.Left.Left);
            
            Assert.AreEqual(leftOfLeftValue, tree.Root.Left.Left.Value);
            Assert.IsNull(tree.Root.Left.Left.Left);
            Assert.IsNull(tree.Root.Left.Left.Right);
        }
        
        [TestCase(5, 3, 4)]
        public void Insert_InOrderLeftRightToCoarseTree_UpdateTree(int rootValue, int leftValue, int rightOfLeftValue)
        {
            var tree = new CoarseGrainedSyncBinaryTree<int>();
            
            tree.Insert(rootValue);
            tree.Insert(leftValue);
            tree.Insert(rightOfLeftValue);
            
            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Right);
            Assert.IsNotNull(tree.Root.Left);
            
            Assert.AreEqual(leftValue, tree.Root.Left.Value);
            Assert.IsNull(tree.Root.Left.Left);
            Assert.IsNotNull(tree.Root.Left.Right);
            
            Assert.AreEqual(rightOfLeftValue, tree.Root.Left.Right.Value);
            Assert.IsNull(tree.Root.Left.Right.Left);
            Assert.IsNull(tree.Root.Left.Right.Right);
        }
        
        [TestCase(5, 7, 6)]
        public void Insert_InOrderRightLeftToCoarseTree_UpdateTree(int rootValue, int rightValue, int leftOfRightValue)
        {
            var tree = new CoarseGrainedSyncBinaryTree<int>();
            
            tree.Insert(rootValue);
            tree.Insert(rightValue);
            tree.Insert(leftOfRightValue);
            
            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Left);
            Assert.IsNotNull(tree.Root.Right);
            
            Assert.AreEqual(rightValue, tree.Root.Right.Value);
            Assert.IsNull(tree.Root.Right.Right);
            Assert.IsNotNull(tree.Root.Right.Left);
            
            Assert.AreEqual(leftOfRightValue, tree.Root.Right.Left.Value);
            Assert.IsNull(tree.Root.Right.Left.Left);
            Assert.IsNull(tree.Root.Right.Left.Right);
        }
        
        [TestCase(5, 7, 9)]
        public void Insert_InOrderRightRightToCoarseTree_UpdateTree(int rootValue, int rightValue, int rightOfRightValue)
        {
            var tree = new CoarseGrainedSyncBinaryTree<int>();
            
            tree.Insert(rootValue);
            tree.Insert(rightValue);
            tree.Insert(rightOfRightValue);
            
            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Left);
            Assert.IsNotNull(tree.Root.Right);
            
            Assert.AreEqual(rightValue, tree.Root.Right.Value);
            Assert.IsNull(tree.Root.Right.Left);
            Assert.IsNotNull(tree.Root.Right.Right);
            
            Assert.AreEqual(rightOfRightValue, tree.Root.Right.Right.Value);
            Assert.IsNull(tree.Root.Right.Right.Left);
            Assert.IsNull(tree.Root.Right.Right.Right);
        }
    }
}