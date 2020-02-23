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
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + FirstVertex.GetHashCode();
                hash = hash * 31 + SecondVertex.GetHashCode();
                return hash;
            }
        }
        
        public override bool Equals(object obj) 
        { 
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Edge) obj); 
        }
        
        private bool Equals(Edge other)
        { 
            return FirstVertex == other.FirstVertex && SecondVertex == other.SecondVertex; 
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