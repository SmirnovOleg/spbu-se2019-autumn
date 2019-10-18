using System;
using System.Collections.Generic;
using System.Threading;

namespace Task02
{
    public static class Algorithms
    {
        public static int[,] RunParallelFloyd(List<Edge> edges, int numVertices)
        {
            // Prepare matrix with distances
            int[,] dist = new int[numVertices, numVertices];
            Utils.FillMatrix(dist, Int32.MaxValue);
            for (int i = 0; i < numVertices; i++)
                dist[i, i] = 0;
            foreach (var edge in edges)
            {
                int u = edge.firstVertex, v = edge.secondVertex, cost = edge.cost;
                dist[u, v] = dist[v, u] = cost;
            }
            
            int totalMatrixSize = numVertices * numVertices;
            int numThreads = Config.NumThreads;
            if (numThreads > totalMatrixSize)
            {
                numThreads = 1;
            }
            int chunkSize = totalMatrixSize / numThreads;
            Thread[] workerThreads = new Thread[numThreads];
            
            for (int pivot = 0; pivot < numVertices; pivot++)
            {
                for (int num = 0; num < numThreads; ++num)
                {
                    int currentChunkStart = num * chunkSize;
                    int currentChunkEnd = currentChunkStart + chunkSize;
                    if (totalMatrixSize - currentChunkStart < 2 * chunkSize) // In case totalMatrixSize % chunkSize != 0
                    {
                        currentChunkEnd = totalMatrixSize;
                    }
                    workerThreads[num] = new Thread(() =>
                    {
                        for (int pos = currentChunkStart; pos < currentChunkEnd; pos++)
                        {
                            int i = pos / numVertices;
                            int j = pos % numVertices;
                            if (dist[i, pivot] != Int32.MaxValue && dist[pivot, j] != Int32.MaxValue)
                            {
                                dist[i, j] = Math.Min(dist[i, j], dist[i, pivot] + dist[pivot, j]);
                            }
                        }
                    });
                    workerThreads[num].Start();
                }
                foreach (Thread thread in workerThreads)
                {
                    thread.Join();
                }
            }

            return dist;
        }
        
        public static int[,] RunFloyd(List<Edge> edges, int numVertices)
        {
            int[,] dist = new int[numVertices, numVertices];
            Utils.FillMatrix(dist, Int32.MaxValue);
            for (int i = 0; i < numVertices; i++)
                dist[i, i] = 0;
            foreach (var edge in edges)
            {
                int u = edge.firstVertex, v = edge.secondVertex, cost = edge.cost;
                dist[u, v] = dist[v, u] = cost;
            }
            
            for (int k = 0; k < numVertices; k++)
            {
                for (int i = 0; i < numVertices; ++i)
                {
                    for (int j = 0; j < numVertices; ++j)
                    {
                        if (dist[i, k] != Int32.MaxValue && dist[k, j] != Int32.MaxValue)
                            dist[i, j] = Math.Min(dist[i, j], dist[i, k] + dist[k, j]);
                    }
                }
            }

            return dist;
        }
    }
}