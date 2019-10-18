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
            int hash = 113;
            hash += (hash * 997) + FirstVertex;
            hash += (hash * 997) + SecondVertex;
            return hash;
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
    
    public class WeightedEdge: Edge
    {
        public readonly int Cost;

        public WeightedEdge(int firstVertex, int secondVertex, int cost) : base(firstVertex, secondVertex)
        {
            Cost = cost;
        }
    }
}