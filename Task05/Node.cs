namespace Task05
{
    internal class Node<T> where T: System.IComparable<T>
    {
        public T Value;
        public Node<T> Left;
        public Node<T> Right;

        public Node(T value)
        {
            Value = value;
            Left = null;
            Right = null;
        }
    }
}