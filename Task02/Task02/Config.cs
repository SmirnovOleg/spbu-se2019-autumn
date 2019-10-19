using System;

namespace Task02
{
    public struct Config
    {
        public static readonly int NumThreads = Environment.ProcessorCount;
        public const int OptimalThreadPoolSize = 10;
        public const int SequentialThreshold = 500;
        
        public const string InputFileName = "input.txt";
        public const string OutputFloydFileName = "floyd.txt";
        public const string OutputPrimFileName = "prim.txt";
        public const string OutputKruskalFileName = "kruskal.txt";

        public const int MinNumVertices = 2000;
        public const int MaxNumVertices = 3000;
        
        public const int MinNumEdges = 100000;
        
        public const int MinEdgeCost = 1;
        public const int MaxEdgeCost = 1000;
    }
}