using System;

namespace Task02
{
    public struct Config
    {
        public static readonly int NumThreads = Environment.ProcessorCount;
        public const int OptimalNumChunks = 10;
        public const int SequentialThreshold = 500;
        
        public const string InputFileName = "data/input.txt";
        public const string OutputFloydFileName = "data/floyd.txt";
        public const string OutputPrimFileName = "data/prim.txt";
        public const string OutputKruskalFileName = "data/kruskal.txt";

        public const int MinNumVertices = 100;
        public const int MaxNumVertices = 500;
        
        public const int MinNumEdges = 1000;
        
        public const int MinEdgeCost = 1;
        public const int MaxEdgeCost = 1000;
    }
}