namespace Task05
{
    class Program
    {
        static void Main()
        {
            BinaryTree<int> tree = new CoarseGrainedSyncBinaryTree<int>(new []{5, 9, 7, 8, 3, 1, 12, 10, 11});
            tree.Print();
            tree.Remove(5);
            tree.Print();
            tree.Remove(9);
            tree.Print();
        }
    }
}