using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DataStructures.Tests
{
    public class CountingComparerTests
    {
        private const int Count = 10000;
        private const int Seed = 1000;
        private const int MaxNumber = 100000;
        private readonly ITestOutputHelper _output;
        private readonly int[] _data;

        public CountingComparerTests(ITestOutputHelper output)
        {
            _output = output;
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

        [Fact]
        public void TestRandomInsertsCompareCallCount()
        {
            var comparer1 = new CountingComparer();
            var comparer2 = new CountingComparer();
            var comparer4 = new CountingComparer();
            var comparer5 = new CountingComparer();
            var tree1 = new AvlTree<int>(comparer1);
            var tree2 = new CompactAvlTree<int>(comparer2);
            var list = new List<int>();
            var set = new SortedSet<int>(comparer5);
            for (int i = 0; i < _data.Length; i++)
            {
                var n = _data[i];
                tree1.Add(n);
                tree2.Add(n);
                set.Add(n);
                var index = list.BinarySearch(n, comparer4);
                if (index < 0)
                {
                    list.Insert(~index, n);
                }
            }

            _output.WriteLine($"AvlTree compare calls per 10000 inserts: {comparer1.Count}");
            _output.WriteLine($"CompactAvlTree compare calls per 10000 inserts: {comparer2.Count}");
            _output.WriteLine($"List compare calls per 10000 inserts: {comparer4.Count}");
            _output.WriteLine($"SortedSet (Red-Black tree) compare calls per 10000 inserts: {comparer5.Count}");

            Assert.True(comparer1.Count > 0);
            Assert.True(comparer2.Count > 0);
            Assert.True(comparer4.Count > 0);
            Assert.True(comparer5.Count > 0);
        }
    }
}
