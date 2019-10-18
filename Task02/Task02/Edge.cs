namespace Task02
{
    public struct Edge
    {
        public int firstVertex;
        public int secondVertex;
        public int cost;

        public Edge(int firstVertex, int secondVertex, int cost)
        {
            this.firstVertex = firstVertex;
            this.secondVertex = secondVertex;
            this.cost = cost;
        }
    }
}