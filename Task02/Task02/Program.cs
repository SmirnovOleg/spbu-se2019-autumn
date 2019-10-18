using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Task02
{
    static class Program
    {
        private static readonly string inputFileName = "input.txt";
        private static readonly List<Tuple<int, int, int>> edges = new List<Tuple<int, int, int>>();
        private static int[,] dist;

        static void Main(string[] args)
        {
            //Utils.GenerateRandomGraph(inputFileName);
            try
            {
                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;
                using StreamReader reader = new StreamReader(Path.Join(projectDirectory, inputFileName));
                var input = reader.ReadLine().Split(' ');
                int n = int.Parse(input[0]);
                int m = int.Parse(input[1]);
                dist = new int[n, n];
                Utils.FillMatrix(dist, 1000000);
                for (int i = 0; i < n; i++)
                    dist[i, i] = 0;
                for (int i = 0; i < m; i++)
                {
                    input = reader.ReadLine().Split(' ');
                    int u = int.Parse(input[0]) - 1;
                    int v = int.Parse(input[1]) - 1;
                    int cost = int.Parse(input[2]);
                    edges.Add(Tuple.Create(u, v, cost));
                    edges.Add(Tuple.Create(v, u, cost));
                    dist[u, v] = dist[v, u] = cost;
                }
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case FileNotFoundException _:
                        Console.WriteLine("No such file or directory");
                        break;
                    case IndexOutOfRangeException _:
                        Console.WriteLine("Wrong data");
                        break;
                    case NullReferenceException _:
                        Console.WriteLine("Wrong format of data");
                        break;
                }
                throw;
            }
            
            Utils.TimeIt(RunParallelFloyd);
            // Utils.PrintMatrix(dist);
        }
        
        public static void RunParallelFloyd()
        {
            int n = dist.GetLength(0);
            int totalSize = n * n;
            
            int numThreads = 20;
            if (numThreads > totalSize)
                numThreads = 1;
            int chunkSize = totalSize / numThreads;

            for (int pivot = 0; pivot < n; pivot++)
            {
                ManualResetEvent allDone = new ManualResetEvent(initialState: false);
                int completed = 0;
                for (int num = 0; num < numThreads; ++num)
                {
                    int currentChunkStart = num * chunkSize;
                    int currentChunkEnd = currentChunkStart + chunkSize;
                    if (totalSize - currentChunkStart < 2 * chunkSize) // In case totalSize % chunkSize != 0
                        currentChunkEnd = totalSize;
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        for (int pos = currentChunkStart; pos < currentChunkEnd; pos++)
                        {
                            int i = pos / n;
                            int j = pos % n;
                            if (dist[i, pivot] != Int32.MaxValue && dist[pivot, j] != Int32.MaxValue)
                                dist[i, j] = Math.Min(dist[i, j], dist[i, pivot] + dist[pivot, j]);
                        }
                        if (Interlocked.Increment(ref completed) == numThreads)
                        {
                            allDone.Set();
                        }
                    });
                }

                allDone.WaitOne();
            }
        }
    }
}