using System;

namespace Task02
{
    public struct Config
    {
        public static readonly int NumThreads = Environment.ProcessorCount;
        public const int OptimalThreadPoolSize = 10;
        public const int SequentialThreshold = 500;
        
        public const string InputFileName = "input.txt";
        public const string OutputFileName = "output.txt";
        
        public const int MinNumVertices = 5000;
        public const int MaxNumVertices = 6000;
        
        public const int MinNumEdges = 1000000;
        
        public const int MinEdgeCost = 1;
        public const int MaxEdgeCost = 100;
    }
}