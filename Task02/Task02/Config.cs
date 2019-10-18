using System;

namespace Task02
{
    public struct Config
    {
        public static readonly int NumThreads = Environment.ProcessorCount;
        public const int OptimalThreadPoolSize = 20;
        
        public const string InputFileName = "input.txt";
        public const string OutputFileName = "output.txt";
        
        public const int MinNumVertices = 1000;
        public const int MaxNumVertices = 1500;
        
        public const int MinNumEdges = 1;
        
        public const int MinEdgeCost = 1;
        public const int MaxEdgeCost = 100;
    }
}