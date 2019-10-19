using System.Collections.Generic;

namespace Task02
{
    public class DisjointSetUnion<T>
    {
        private readonly Dictionary<T, T> parent = new Dictionary<T, T>();
        private readonly Dictionary<T, int> rank = new Dictionary<T, int>();
        
        public DisjointSetUnion(List<T> items)
        {
            foreach (var item in items)
            {
                parent[item] = item;
                rank[item] = 0;
            }
        }

        public T GetParent(T vertex)
        {
            if (Equals(vertex, parent[vertex]))
            {
                return vertex;
            }
            return parent[vertex] = GetParent(parent[vertex]);
        }

        public void Unite(T first, T second)
        {
            first = GetParent(first);
            second = GetParent(second);
            if (!Equals(first, second))
            {
                if (rank[first] < rank[second])
                {
                    Utils.Swap(ref first, ref second);
                }
                parent[second] = first;
                if (rank[first] == rank[second])
                {
                    ++rank[first];
                }
            }
        }
    }
}