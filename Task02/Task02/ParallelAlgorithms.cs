using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Task02
{
    public static class ParallelAlgorithms
    {
        public static int RunParallelKruskal(List<WeightedEdge> edges, int numVertices)
        {
            int totalCost = 0;
            var edgesArray = edges.ToArray();
            RunParallelQuickSort(edgesArray);
            var dsu = new DisjointSetUnion<int>(Enumerable.Range(0, numVertices).ToArray());
            foreach (var weightedEdge in edgesArray)
            {
                if (!Equals(dsu.GetParent(weightedEdge.FirstVertex), dsu.GetParent(weightedEdge.SecondVertex)))
                {
                    totalCost += weightedEdge.Cost;
                    dsu.Unite(weightedEdge.FirstVertex, weightedEdge.SecondVertex);
                }
            }
            return totalCost;
        }

        public static void RunParallelQuickSort<T>(T[] items) where T : IComparable<T>
        {
            ParallelQuickSort(items, 0, items.Length);
        }

        private static void ParallelQuickSort<T>(T[] items, int left, int right) where T: IComparable<T>
        {
            if (right - left <= 1)
            {
                return;
            }
            int pivot = SequentialAlgorithms.Partition(items, left, right);
            if (right - left > Config.SequentialThreshold)
            {
                Task firstTask = Task.Run(() => SequentialAlgorithms.SequentialQuickSort(items, left, pivot));
                Task secondTask = Task.Run(() => SequentialAlgorithms.SequentialQuickSort(items, pivot + 1, right));
                Task.WaitAll(firstTask, secondTask);
            }
            else
            {
                SequentialAlgorithms.SequentialQuickSort(items, left, pivot);
                SequentialAlgorithms.SequentialQuickSort(items, pivot + 1, right);
            }
        }
        
        public static int RunParallelPrim(List<WeightedEdge> edges, int numVertices)
        {
            // Prepare arrays for algorithm            
            int[] minEdgeToMst = new int[numVertices];
            Utils.Fill(minEdgeToMst, Int32.MaxValue);
            minEdgeToMst[0] = 0;
            
            bool[] used = new bool[numVertices];
            Utils.Fill(used, false);
            
            var edgesMap = new Dictionary<Edge, int>();
            edges.ForEach(weightedEdge =>
            {
                var edge = new Edge(weightedEdge.FirstVertex, weightedEdge.SecondVertex);
                edgesMap.Add(edge, weightedEdge.Cost);
            });
            
            int totalCost = 0;
            
            // Prepare threads and chunks info
            int numThreads = Config.OptimalNumChunks;
            if (numThreads > numVertices)
            {
                numThreads = 1;
            }
            int chunkSize = numVertices / numThreads;
            
            // Run global cycle throw all `numVertices` phases
            for (int i = 0; i < numVertices; i++)
            {
                AutoResetEvent allDone = new AutoResetEvent(false);
                int completed = 0;
                
                // Find the nearest vertex to current MST using ThreadPool
                int[] targetByChunks = new int[numThreads];
                for (int num = 0; num < numThreads; num++)
                {
                    int currentChunkStart = num * chunkSize;
                    int currentChunkEnd = currentChunkStart + chunkSize;
                    if (numVertices - currentChunkStart < 2 * chunkSize) // In case numVertices % chunkSize != 0
                        currentChunkEnd = numVertices;
                    targetByChunks[num] = -1; // In case of disconnected graph it will help
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var id = (int) state;
                        for (int currentVertex = currentChunkStart; currentVertex < currentChunkEnd; currentVertex++)
                        {
                            if ((targetByChunks[id] == -1
                                    || minEdgeToMst[currentVertex] < minEdgeToMst[targetByChunks[id]]) 
                                && !used[currentVertex])
                            {
                                targetByChunks[id] = currentVertex;
                            }
                        }
                        if (Interlocked.Increment(ref completed) == numThreads)
                        {
                            allDone.Set();
                        }
                    }, num);
                }
                allDone.WaitOne();
                int targetVertex = -1;
                foreach (var chunkResult in targetByChunks)
                {
                    if (chunkResult != -1
                        && (targetVertex == -1 || minEdgeToMst[chunkResult] < minEdgeToMst[targetVertex])
                        && !used[chunkResult])
                    {
                        targetVertex = chunkResult;
                    }
                }
                if (minEdgeToMst[targetVertex] == Int32.MaxValue)
                {
                    // Graph is disconnected. Add new MST to forest
                    minEdgeToMst[targetVertex] = 0;
                }
                
                // Add selected vertex to MST
                used[targetVertex] = true;
                totalCost += minEdgeToMst[targetVertex];
                
                // Update distances from all other vertices to current MST using ThreadPool
                completed = 0;
                for (int num = 0; num < numThreads; num++)
                {
                    int currentChunkStart = num * chunkSize;
                    int currentChunkEnd = currentChunkStart + chunkSize;
                    if (numVertices - currentChunkStart < 2 * chunkSize) // In case numVertices % chunkSize != 0
                        currentChunkEnd = numVertices;
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        for (int otherVertex = currentChunkStart; otherVertex < currentChunkEnd; otherVertex++)
                        {
                            var edge = new Edge(targetVertex, otherVertex);
                            if (!edgesMap.ContainsKey(edge)) 
                                continue;
                            var currentCost = edgesMap[edge];
                            if (currentCost < minEdgeToMst[otherVertex])
                            {
                                minEdgeToMst[otherVertex] = currentCost;
                            }
                        }
                        if (Interlocked.Increment(ref completed) == numThreads)
                        {
                            allDone.Set();
                        }
                    });
                }
                allDone.WaitOne();
                //Utils.PrintArray(minEdgeToMST);
                //Console.WriteLine(totalCost);
            }

            return totalCost;
        }
        
        public static int[,] RunParallelFloyd(List<WeightedEdge> edges, int numVertices)
        {
            // Prepare matrix with distances
            int[,] dist = new int[numVertices, numVertices];
            Utils.FillMatrix(dist, Int32.MaxValue);
            for (int i = 0; i < numVertices; i++)
                dist[i, i] = 0;
            foreach (var edge in edges)
            {
                int u = edge.FirstVertex, v = edge.SecondVertex, cost = edge.Cost;
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
    }
}