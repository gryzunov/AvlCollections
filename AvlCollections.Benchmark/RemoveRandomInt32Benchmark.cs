using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace AvlCollections.Benchmark
{
    [LegacyJitX86Job, RyuJitX64Job]
    public class RemoveRandomInt32Benchmark
    {
        private const int Seed = 1000;
        private const int MaxNumber = 100000;
        private int[] _data;
        private AvlTree<int> _tree;
        private CompactAvlTree<int> _compactTree;
        private List<int> _list;
        private SortedSet<int> _set;

        [Params(100, 1000, 10000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random(Seed);
            var set = new HashSet<int>(Count);
            _data = new int[Count];
            for (int i = 0; i < Count; i++)
            {
                int number;
                do
                {
                    number = random.Next(MaxNumber);
                } while (!set.Add(number));
                _data[i] = number;
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _tree = new AvlTree<int>();
            _compactTree = new CompactAvlTree<int>();
            _list = new List<int>();
            _set = new SortedSet<int>();
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                _tree.Add(number);
                _compactTree.Add(number);
                _set.Add(number);
                _list.Add(number);
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
                throw new Exception($"Remove error in AvlTree. Expected count={Count}, actual count={count}");
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
                throw new Exception($"Remove error in CompactAvlTree. Expected count={Count}, actual count={count}");
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
                throw new Exception($"Remove error in RBTree. Expected count={Count}, actual count={count}");
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
                throw new Exception($"Remove error in List. Expected count={Count}, actual count={count}");
            }
            return count;
        }
    }
}
