using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace DataStructures.Benchmark
{
    [LegacyJitX86Job, RyuJitX64Job]
    public class AvlTreeVsRBTreeInsertRandom
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
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = random.Next(MaxNumber);
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
        public void TestAvl2()
        {
            var tree = new AvlTree2<int>();
            for (int i = 0; i < _data.Length; i++)
            {
                var n = _data[i];
                tree.Add(n);
            }
        }

        [Benchmark]
        public void TestRb()
        {
            var dic = new SortedSet<int>();
            for (int i = 0; i < _data.Length; i++)
            {
                var n = _data[i];
                dic.Add(n);
            }
        }

        [Benchmark]
        public void TestList()
        {
            var list = new List<int>();
            for (int i = 0; i < _data.Length; i++)
            {
                var n = _data[i];
                var index = list.BinarySearch(n);
                if (index < 0)
                {
                    list.Insert(~index, n);
                }
            }
        }
    }
}
