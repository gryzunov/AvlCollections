using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace AvlCollections.Benchmark
{
    [MemoryDiagnoser]
    [LegacyJitX86Job, RyuJitX64Job]
    public class InsertRandomDecimalBenchmark
    {
        private const int Seed = 1000;
        private const int MaxNumber = 100000;
        private decimal[] _data;

        [Params(100, 1000, 10000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _data = new decimal[Count];
            var random = new Random(Seed);
            var set = new HashSet<int>(Count);
            for (int i = 0; i < _data.Length; i++)
            {
                var number = random.Next(MaxNumber);
                if (!set.Add(number))
                {
                    continue;
                }
                _data[i] = number;
            }
        }

        [Benchmark]
        public void TestAvlTree()
        {
            var tree = new AvlTree<decimal>();
            for (int i = 0; i < _data.Length; i++)
            {
                var n = _data[i];
                tree.Add(n);
            }
        }

        [Benchmark]
        public void TestCompactAvlTree()
        {
            var tree = new CompactAvlTree<decimal>();
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                tree.Add(number);
            }
        }

        [Benchmark(Baseline = true)]
        public void TestRBTree()
        {
            var set = new SortedSet<decimal>();
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                set.Add(number);
            }
        }

        [Benchmark]
        public void TestList()
        {
            var list = new List<decimal>();
            for (int i = 0; i < _data.Length; i++)
            {
                var number = _data[i];
                var index = list.BinarySearch(number);
                if (index < 0)
                {
                    list.Insert(~index, number);
                }
            }
        }
    }
}
