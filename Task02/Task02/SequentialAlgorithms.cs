using System;
using System.Collections.Generic;

namespace Task02
{
    public class SequentialAlgorithms
    {
        public static int RunPrim(List<WeightedEdge> edges, int numVertices)
        {
            // Prepare arrays for algorithm            
            int[] minEdgeToMST = new int[numVertices];
            Utils.Fill(minEdgeToMST, Int32.MaxValue);
            minEdgeToMST[0] = 0;
            
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
                    if ((targetVertex == -1 || minEdgeToMST[currentVertex] < minEdgeToMST[targetVertex]) 
                        && !used[currentVertex])
                    {
                        targetVertex = currentVertex;
                    }
                }
                if (minEdgeToMST[targetVertex] == Int32.MaxValue)
                {
                    // Graph is disconnected. Add new MST to forest
                    minEdgeToMST[targetVertex] = 0;
                }
                
                // Add selected vertex to MST
                used[targetVertex] = true;
                totalCost += minEdgeToMST[targetVertex];
                
                // Update distances from all other vertices to current MST
                for (int otherVertex = 0; otherVertex < numVertices; otherVertex++)
                {
                    var edge = new Edge(targetVertex, otherVertex);
                    if (!edgesMap.ContainsKey(edge)) 
                        continue;
                    var currentCost = edgesMap[edge];
                    if (currentCost < minEdgeToMST[otherVertex])
                    {
                        minEdgeToMST[otherVertex] = currentCost;
                    }
                }
            }

            return totalCost;
        }
        
        public static int[,] RunFloyd(List<WeightedEdge> edges, int numVertices)
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
    }
}