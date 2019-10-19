using System.Collections.Generic;

namespace Task02
{
    public class DisjointSetUnion<T>
    {
        private readonly Dictionary<T, T> _parent = new Dictionary<T, T>();
        private readonly Dictionary<T, int> _rank = new Dictionary<T, int>();
        
        public DisjointSetUnion(T[] items)
        {
            foreach (var item in items)
            {
                _parent[item] = item;
                _rank[item] = 0;
            }
        }

        public T GetParent(T vertex)
        {
            if (Equals(vertex, _parent[vertex]))
            {
                return vertex;
            }
            return _parent[vertex] = GetParent(_parent[vertex]);
        }

        public void Unite(T first, T second)
        {
            first = GetParent(first);
            second = GetParent(second);
            if (!Equals(first, second))
            {
                if (_rank[first] < _rank[second])
                {
                    Utils.Swap(ref first, ref second);
                }
                _parent[second] = first;
                if (_rank[first] == _rank[second])
                {
                    ++_rank[first];
                }
            }
        }
    }
}