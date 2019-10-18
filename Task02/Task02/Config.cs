using System;

namespace Task02
{
    public struct Config
    {
        public static readonly int NumThreads = Environment.ProcessorCount;
        public const int OptimalThreadPoolSize = 4;
        
        public const string InputFileName = "input.txt";
        public const string OutputFileName = "output.txt";
        
        public const int MinNumVertices = 3;
        public const int MaxNumVertices = 5;
        
        public const int MinNumEdges = 1;
        
        public const int MinEdgeCost = 1;
        public const int MaxEdgeCost = 10;
    }
}