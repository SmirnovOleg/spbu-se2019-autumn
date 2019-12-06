using NUnit.Framework;
using Task05;

namespace Tests.FineGrained
{
    [TestFixture]
    public class RemovingTests
    {
        [TestCase(0)]
        public void Remove_RootInFineTree_ReturnEmptyTree(int value)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(value);
            
            tree.Remove(value); 
            
            Assert.IsNull(tree.Root);
        }
        
        [TestCase(0)]
        public void Remove_NonExistentNodeInFineTree_ReturnEmptyTree(int value)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(value);
            
            Assert.DoesNotThrow(() => tree.Remove(value + 1));
            
            Assert.AreEqual(value, tree.Root.Value);
            Assert.IsNull(tree.Root.Right);
            Assert.IsNull(tree.Root.Left);
        }
        
        [TestCase(5, 3, 1)]
        public void Remove_RootInFineTree_SetNewRootFromLeft(int rootValue, int leftValue, int leftOfLeftValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(leftValue);
            tree.Insert(leftOfLeftValue);
            
            tree.Remove(rootValue);

            Assert.AreEqual(leftValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Right);
            Assert.IsNotNull(tree.Root.Left);
            
            Assert.AreEqual(leftOfLeftValue, tree.Root.Left.Value);
            Assert.IsNull(tree.Root.Left.Right);
            Assert.IsNull(tree.Root.Left.Left);
        }
        
        [TestCase(5, 7, 9)]
        public void Remove_RootInFineTree_SetNewRootFromRight(int rootValue, int rightValue, int rightOfRightValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(rightValue);
            tree.Insert(rightOfRightValue);
            
            tree.Remove(rootValue);

            Assert.AreEqual(rightValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Left);
            Assert.IsNotNull(tree.Root.Right);
            
            Assert.AreEqual(rightOfRightValue, tree.Root.Right.Value);
            Assert.IsNull(tree.Root.Right.Right);
            Assert.IsNull(tree.Root.Right.Left);
        }
        
        [TestCase(5, 2, 4, 3)]
        public void Remove_RootInFineTree_UpdateRootFromLeftNeighbor(int rootValue, int leftValue, 
            int rightOfLeftValue, int leftOfRightOfLeftValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(leftValue);
            tree.Insert(rightOfLeftValue);
            tree.Insert(leftOfRightOfLeftValue);
            
            tree.Remove(rootValue);

            Assert.AreEqual(rightOfLeftValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Right);
            Assert.IsNotNull(tree.Root.Left);
            
            Assert.AreEqual(leftValue, tree.Root.Left.Value);
            Assert.IsNull(tree.Root.Left.Left);
            Assert.IsNotNull(tree.Root.Left.Right);
            
            Assert.AreEqual(leftOfRightOfLeftValue, tree.Root.Left.Right.Value);
            Assert.IsNull(tree.Root.Left.Right.Left);
            Assert.IsNull(tree.Root.Left.Right.Right);
        }
        
        [TestCase(5, 8, 6, 7)]
        public void Remove_RootInFineTree_UpdateRootFromRightNeighbor(int rootValue, int rightValue, 
            int leftOfRightValue, int rightOfLeftOfOfRightValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(rightValue);
            tree.Insert(leftOfRightValue);
            tree.Insert(rightOfLeftOfOfRightValue);
            
            tree.Remove(rootValue);

            Assert.AreEqual(leftOfRightValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Left);
            Assert.IsNotNull(tree.Root.Right);
            
            Assert.AreEqual(rightValue, tree.Root.Right.Value);
            Assert.IsNull(tree.Root.Right.Right);
            Assert.IsNotNull(tree.Root.Right.Left);
            
            Assert.AreEqual(rightOfLeftOfOfRightValue, tree.Root.Right.Left.Value);
            Assert.IsNull(tree.Root.Right.Left.Left);
            Assert.IsNull(tree.Root.Right.Left.Right);
        }
        
        [TestCase(5, 8, 7, 6)]
        public void Remove_InnerNodeInFineTree_UpdateFromRight(int rootValue, int rightValue, 
            int leftOfRightValue, int leftOfLeftOfRightValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(rightValue);
            tree.Insert(leftOfRightValue);
            tree.Insert(leftOfLeftOfRightValue);
            
            tree.Remove(rightValue);

            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Left);
            Assert.IsNotNull(tree.Root.Right);
            
            Assert.AreEqual(leftOfRightValue, tree.Root.Right.Value);
            Assert.IsNull(tree.Root.Right.Right);
            Assert.IsNotNull(tree.Root.Right.Left);
            
            Assert.AreEqual(leftOfLeftOfRightValue, tree.Root.Right.Left.Value);
            Assert.IsNull(tree.Root.Right.Left.Left);
            Assert.IsNull(tree.Root.Right.Left.Right);
        }
        
        [TestCase(5, 1, 3, 4)]
        public void Remove_InnerNodeInFineTree_UpdateFromLeft(int rootValue, int leftValue, 
            int rightOfLeftValue, int rightOfRightOfLeftValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(leftValue);
            tree.Insert(rightOfLeftValue);
            tree.Insert(rightOfRightOfLeftValue);
            
            tree.Remove(leftValue);

            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Right);
            Assert.IsNotNull(tree.Root.Left);
            
            Assert.AreEqual(rightOfLeftValue, tree.Root.Left.Value);
            Assert.IsNull(tree.Root.Left.Left);
            Assert.IsNotNull(tree.Root.Left.Right);
            
            Assert.AreEqual(rightOfRightOfLeftValue, tree.Root.Left.Right.Value);
            Assert.IsNull(tree.Root.Left.Right.Left);
            Assert.IsNull(tree.Root.Left.Right.Right);
        }
        
        [TestCase(1, 2)]
        public void Remove_RightLeafInFineTree_UpdateTree(int rootValue, int rightValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(rightValue);
            
            tree.Remove(rightValue);

            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Left);
            Assert.IsNull(tree.Root.Right);
        }
        
        [TestCase(2, 1)]
        public void Remove_LeftLeafInFineTree_UpdateTree(int rootValue, int leftValue)
        {
            var tree = new FineGrainedSyncBinaryTree<int>();
            tree.Insert(rootValue);
            tree.Insert(leftValue);
            
            tree.Remove(leftValue);

            Assert.AreEqual(rootValue, tree.Root.Value);
            Assert.IsNull(tree.Root.Left);
            Assert.IsNull(tree.Root.Right);
        }
    }
}