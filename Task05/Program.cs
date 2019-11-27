namespace Task05
{
    class Program
    {
        static void Main()
        {
            BinaryTree<int> tree = new FineGrainedSyncBinaryTree<int>(new []{5, 9, 7, 8, 3, 1, 12, 10, 11});
            tree.Print();
            tree.Remove(5);
            tree.Print();
            tree.Remove(10);
            tree.Print();
        }
    }
}