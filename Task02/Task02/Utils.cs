using System;
using System.IO;

namespace Task02
{
    public static class Utils
    {
        public static void TimeIt(Action func)
        {
            long start = DateTime.UtcNow.Ticks;
            func();
            long end = DateTime.UtcNow.Ticks;
            Console.WriteLine($"Total time: {(end - start)} ticks");
        }

        public static bool MatrixEquals<T>(T[,] main, T[,] other) where T: IComparable
        {
            bool isEqual = true;
            for (int i = 0; i < main.GetLength(0); i++)
            {
                for (int j = 0; j < main.GetLength(1); j++)
                {
                    if (!Equals(main[i, j], other[i, j]))
                    {
                        isEqual = false;
                        break;
                    }
                }
                if (!isEqual)
                {
                    break;
                }
            }

            return isEqual;
        }
        
        public static void Fill<T>(T[] arr, T value) {
            for (int i = 0; i < arr.Length; ++i) {
                arr[i] = value;
            }
        }

        public static void FillMatrix<T>(T[,] matrix, T value)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = value;
                }
            }
        }
        
        public static void PrintArray<T>(T[] array)
        {
            foreach (var current in array)
            {
                Console.Write($"{current} ");
            }
            Console.WriteLine();
        }
        
        public static void PrintMatrix<T>(T[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j]} ");
                }
                Console.WriteLine();
            }
        }

        public static void GenerateRandomGraph(string filename)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;
            string finalPath = Path.Join(projectDirectory, filename);
            using StreamWriter writer = File.CreateText(finalPath);
            
            var random = new Random();
            int n = random.Next(Config.MinNumVertices, Config.MaxNumVertices + 1);
            int m = random.Next(Config.MinNumEdges, n * (n - 1) / 2 + 1);
            int[,] graph = new int[n, n];
            int[] degree = new int[n];
            
            writer.WriteLine($"{n} {m}");
            for (int i = 0; i < m; i++)
            {
                int u, v;
                // Choose first vertex
                while (degree[u = random.Next(n)] == n - 1) {}
                // Choose second vertex
                while (graph[u, v = random.Next(n)] != 0 || v == u) {}
                // Update degrees and adjacency matrix
                degree[v]++;
                degree[u]++;
                graph[u, v] = graph[v, u] = 1;
                int cost = random.Next(Config.MinEdgeCost, Config.MaxEdgeCost + 1);
                writer.WriteLine($"{Math.Min(u, v) + 1} {Math.Max(u, v) + 1} {cost}");
            }
        }
    }
}