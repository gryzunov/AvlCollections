using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace AvlCollections.Benchmark
{
    [LegacyJitX86Job, RyuJitX64Job]
    public class RemoveRandomDecimalBenchmark
    {
        private const int Seed = 1000;
        private const int MaxNumber = 100000;
        private decimal[] _data;
        private AvlTree<decimal> _tree;
        private CompactAvlTree<decimal> _compactTree;
        private List<decimal> _list;
        private SortedSet<decimal> _set;

        [Params(100, 1000, 10000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random(Seed);
            var set = new HashSet<int>(Count);
            _data = new decimal[Count];
            _tree = new AvlTree<decimal>();
            _compactTree = new CompactAvlTree<decimal>();
            _list = new List<decimal>();
            _set = new SortedSet<decimal>();
            for (int i = 0; i < Count; i++)
            {
                while (true)
                {
                    var number = random.Next(MaxNumber);
                    if (!set.Add(number))
                    {
                        continue;
                    }
                    _data[i] = number;
                    _tree.Add(number);
                    _compactTree.Add(number);
                    _set.Add(number);
                    _list.Add(number);
                    break;
                }
            }
            _list.Sort();
        }

        [Benchmark]
        public int TestAvlTree()
        {
            var count = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                if (_tree.Remove(number))
                {
                    ++count;
                }
            }
            if (count != Count)
            {
                throw new Exception("Remove error in AvlTree");
            }
            return count;
        }

        [Benchmark]
        public int TestCompactAvlTree()
        {
            var count = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                if (_compactTree.Remove(number))
                {
                    ++count;
                }
            }
            if (count != Count)
            {
                throw new Exception("Remove error in CompactAvlTree");
            }
            return count;
        }

        [Benchmark(Baseline = true)]
        public int TestRBTree()
        {
            var count = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                if (_set.Remove(number))
                {
                    ++count;
                }
            }
            if (count != Count)
            {
                throw new Exception("Remove error in RBTree");
            }
            return count;
        }

        [Benchmark]
        public int TestList()
        {
            var count = 0;
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                var index = _list.BinarySearch(number);
                if (index >= 0)
                {
                    _list.RemoveAt(index);
                    ++count;
                }
            }
            if (count != Count)
            {
                throw new Exception("Remove error in List");
            }
            return count;
        }
    }
}
