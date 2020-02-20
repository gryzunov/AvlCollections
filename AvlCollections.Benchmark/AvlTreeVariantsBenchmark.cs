using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace AvlCollections.Benchmark
{
    [MemoryDiagnoser]
    [LegacyJitX86Job, RyuJitX64Job]
    public class AvlTreeVariantsBenchmark
    {
        private const int Seed = 1000;
        private const int MaxNumber = 100000;
        private int[] _data;

        [Params(100, 1000, 10000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _data = new int[Count];
            var random = new Random(Seed);
            var set = new HashSet<int>();
            for (int i = 0; i < _data.Length; i++)
            {
                int num;
                do
                {
                    num = random.Next(MaxNumber);
                } while (!set.Add(num));
                _data[i] = num;
            }
        }

        [Benchmark]
        public void TestAvl()
        {
            var tree = new AvlTree<int>();
            for (int i = 0; i < _data.Length; i++)
            {
                var n = _data[i];
                tree.Add(n);
            }
        }

        [Benchmark]
        public void TestCompactAvl()
        {
            var tree = new CompactAvlTree<int>();
            for (int i = 0; i < _data.Length; i++)
            {
                var n = _data[i];
                tree.Add(n);
            }
        }
    }
}
