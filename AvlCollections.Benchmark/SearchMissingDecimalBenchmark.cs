using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace AvlCollections.Benchmark
{
    [LegacyJitX86Job, RyuJitX64Job]
    public class SearchMissingDecimalBenchmark
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
                int number;
                do
                {
                    number = random.Next(MaxNumber) | 1;
                } while (!set.Add(number));
                _data[i] = number;
                var num1 = (decimal) (number + 1);
                _tree.Add(num1);
                _compactTree.Add(num1);
                _set.Add(num1);
                _list.Add(num1);
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
                if (_tree.Contains(number))
                {
                    ++count;
                }
            }
            if (count != 0)
            {
                throw new Exception("Search error in AvlTree");
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
                if (_compactTree.Contains(number))
                {
                    ++count;
                }
            }
            if (count != 0)
            {
                throw new Exception("Search error in CompactAvlTree");
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
                if (_set.Contains(number))
                {
                    ++count;
                }
            }
            if (count != 0)
            {
                throw new Exception("Search error in RBTree");
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
                if (_list.BinarySearch(number) >= 0)
                {
                    ++count;
                }
            }
            if (count != 0)
            {
                throw new Exception("Search error in List");
            }
            return count;
        }
    }
}
