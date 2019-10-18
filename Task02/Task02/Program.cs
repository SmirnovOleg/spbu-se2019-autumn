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
                List<Edge> edges = new List<Edge>();

                int numVertices = int.Parse(input[0]);
                int numEdges = int.Parse(input[1]);

                for (int i = 0; i < numEdges; i++)
                {
                    input = reader.ReadLine().Split(' ');
                    int u = int.Parse(input[0]) - 1;
                    int v = int.Parse(input[1]) - 1;
                    int cost = int.Parse(input[2]);
                    edges.Add(new Edge(u, v, cost));
                    edges.Add(new Edge(v, u, cost));
                }
                
                /*int[,] dist1 = Algorithms.RunParallelFloyd(edges, numVertices);
                int[,] dist2 = Algorithms.RunFloyd(edges, numVertices);
                Utils.PrintMatrix(dist1);
                Console.WriteLine();
                Utils.PrintMatrix(dist2);
                Console.WriteLine($"Equals: {Utils.MatrixEquals(dist1, dist2)}");*/
                
                //int cost1 = Algorithms.RunParallelPrim(edges, numVertices);
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

                throw;
            }
        }
    }
}