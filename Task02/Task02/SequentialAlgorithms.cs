using System;
using System.Collections.Generic;

namespace Task02
{
    public class SequentialAlgorithms
    {
        public static int RunSequentialPrim(List<WeightedEdge> edges, int numVertices)
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
            
            // Run global cycle throw all `numVertices` phases
            for (int i = 0; i < numVertices; i++)
            {
                int targetVertex = -1;
                for (int currentVertex = 0; currentVertex < numVertices; currentVertex++)
                {
                    if ((targetVertex == -1 || minEdgeToMst[currentVertex] < minEdgeToMst[targetVertex]) 
                        && !used[currentVertex])
                    {
                        targetVertex = currentVertex;
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
                
                // Update distances from all other vertices to current MST
                for (int otherVertex = 0; otherVertex < numVertices; otherVertex++)
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
            }

            return totalCost;
        }
        
        public static int[,] RunSequentialFloyd(List<WeightedEdge> edges, int numVertices)
        {
            int[,] dist = new int[numVertices, numVertices];
            Utils.FillMatrix(dist, Int32.MaxValue);
            for (int i = 0; i < numVertices; i++)
                dist[i, i] = 0;
            foreach (var edge in edges)
            {
                int u = edge.FirstVertex, v = edge.SecondVertex, cost = edge.Cost;
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

        public static void RunSequentialQuickSort<T>(T[] items) where T : IComparable<T>
        {
            SequentialQuickSort(items, 0, items.Length);
        }

        internal static void SequentialQuickSort<T>(T[] items, int left, int right) where T : IComparable<T>
        {
            if (left == right) return;
            int pivot = Partition(items, left, right);
            SequentialQuickSort(items, left, pivot);
            SequentialQuickSort(items, pivot + 1, right);
        }

        private static readonly Random RandomIndex = new Random();
        internal static int Partition<T>(T[] items, int left, int right) where T : IComparable<T>
        {
            int pivotPos = RandomIndex.Next(left, right);
            T pivotValue = items[pivotPos];
            Utils.Swap(ref items[right - 1], ref items[pivotPos]);
            
            int current = left;
            for (int i = left; i < right - 1; ++i)
            {
                if (items[i].CompareTo(pivotValue) < 0)
                {
                    Utils.Swap(ref items[i], ref items[current++]);
                }
            }
            Utils.Swap(ref items[right - 1], ref items[current]);
            
            return current;
        }
    }
}