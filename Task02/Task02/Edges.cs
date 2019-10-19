using System;

namespace Task02
{
    public class Edge
    {
        public readonly int FirstVertex;
        public readonly int SecondVertex;

        public Edge(int firstVertex, int secondVertex)
        {
            FirstVertex = firstVertex;
            SecondVertex = secondVertex;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(FirstVertex, SecondVertex);
        }
        
        public override bool Equals(object obj) 
        { 
            return Equals(obj as Edge); 
        }
        
        private bool Equals(Edge obj)
        { 
            return obj != null && obj.GetHashCode() == GetHashCode(); 
        }
    }
    
    public class WeightedEdge: Edge, IComparable<WeightedEdge>
    {
        public readonly int Cost;

        public WeightedEdge(int firstVertex, int secondVertex, int cost) : base(firstVertex, secondVertex)
        {
            Cost = cost;
        }

        public int CompareTo(WeightedEdge other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Cost.CompareTo(other.Cost);
        }
    }
}