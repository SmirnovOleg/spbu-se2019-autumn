using System;
using System.Collections.Generic;
using System.IO;

namespace Task02
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Utils.GenerateRandomGraph(Config.InputFileName);

                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;
                using StreamReader reader = new StreamReader(Path.Join(projectDirectory, Config.InputFileName));
                
                var input = reader.ReadLine().Split(' ');
                List<WeightedEdge> edges = new List<WeightedEdge>();
                int numVertices = int.Parse(input[0]);
                int numEdges = int.Parse(input[1]);
                for (int i = 0; i < numEdges; i++)
                {
                    input = reader.ReadLine().Split(' ');
                    int u = int.Parse(input[0]) - 1;
                    int v = int.Parse(input[1]) - 1;
                    int cost = int.Parse(input[2]);
                    edges.Add(new WeightedEdge(u, v, cost));
                    edges.Add(new WeightedEdge(v, u, cost));
                }

                using StreamWriter writer1 = File.CreateText(Path.Join(projectDirectory, Config.OutputFloydFileName));
                int[,] dist = ParallelAlgorithms.RunParallelFloyd(edges, numVertices);
                Utils.PrintMatrix(dist, writer1);
                
                using StreamWriter writer2 = File.CreateText(Path.Join(projectDirectory, Config.OutputPrimFileName));
                int cost1 = ParallelAlgorithms.RunParallelPrim(edges, numVertices);
                writer2.WriteLine($"{cost1}");

                using StreamWriter writer3 = File.CreateText(Path.Join(projectDirectory, Config.OutputKruskalFileName));
                int cost2 = ParallelAlgorithms.RunParallelKruskal(edges, numVertices);
                writer3.WriteLine($"{cost2}");
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case FileNotFoundException _:
                        Console.WriteLine("No such file or directory: 'input.txt'");
                        break;
                    case IndexOutOfRangeException _:
                        Console.WriteLine("Wrong data - check your graph");
                        break;
                    case NullReferenceException _:
                        Console.WriteLine("Wrong format of data");
                        break;
                }
            }
        }
    }
}