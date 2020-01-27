using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace DataStructures.Benchmark
{
    [MemoryDiagnoser]
    [LegacyJitX86Job, RyuJitX64Job]
    public class AvlTreeWalkCompare
    {
        private const int Seed = 1000;
        private const int MaxNumber = 100000;
        private AvlTree<int> _tree;

        [Params(100, 1000, 10000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random(Seed);
            var set = new HashSet<int>(Count);
            _tree = new AvlTree<int>();
            for (int i = 0; i < Count; i++)
            {
                while (true)
                {
                    var number = random.Next(MaxNumber);
                    if (!set.Add(number))
                    {
                        continue;
                    }
                    _tree.Add(number);
                    break;
                }
            }
        }

        [Benchmark(Baseline = true)]
        public long TreeWalkDelegate()
        {
            long sum = 0;
            _tree.InOrderTreeWalk(item =>
            {
                sum += item;
                return true;
            });
            return sum;
        }

        [Benchmark]
        public long TreeWalkIncremental()
        {
            long sum = 0;
            foreach (var item in _tree)
            {
                sum += item;
            }
            return sum;
        }
    }
}
