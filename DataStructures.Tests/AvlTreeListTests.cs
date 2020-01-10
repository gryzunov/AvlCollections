using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DataStructures.Tests
{
    public class AvlTreeListTests
    {
        private const int ItemCount = 100;

        private readonly int[] _values;
        private readonly int[] _sortedValues;

        public AvlTreeListTests()
        {
            var random = new Random();
            var set = new HashSet<int>();
            _values = new int[ItemCount];
            _sortedValues = new int[ItemCount];
            for (int i = 0; i < _values.Length; i++)
            {
                int number;
                do
                {
                    number = random.Next(1000);
                } while (!set.Add(number));
                _values[i] = number;
                _sortedValues[i] = number;
            }
            Array.Sort(_sortedValues);
        }

        [Fact]
        public void TestEmptyList()
        {
            var tree = new AvlTreeList<int, int>();
            Assert.Empty(tree);
            Assert.Null(tree.First);
            Assert.Null(tree.Last);
        }

        [Fact]
        public void TestListWithSingleItem()
        {
            var tree = new AvlTreeList<int, int>();
            var added = tree.Add(1, 1);
            Assert.True(added);
            _ = Assert.Single(tree);
            Assert.NotNull(tree.First);
            Assert.NotNull(tree.Last);
            Assert.Same(tree.First, tree.Last);
        }

        [Fact]
        public void RandomInsertTest()
        {
            var tree = new AvlTreeList<int, int>();
            AddValues(tree);
            Assert.Equal(ItemCount, tree.Count);
            Assert.Equal(_sortedValues[0], tree.First.Key);
            Assert.Equal(_sortedValues[ItemCount - 1], tree.Last.Key);
            Assert.Equal(_sortedValues[1], tree.First.Next.Key);
            Assert.Equal(_sortedValues[ItemCount - 2], tree.Last.Prev.Key);
            Assert.Equal(_sortedValues[2], tree.First.Next.Next.Key);
            Assert.Equal(_sortedValues[ItemCount - 3], tree.Last.Prev.Prev.Key);
        }

        [Fact]
        public void RemoveSingleItem()
        {
            var tree = new AvlTreeList<int, int>()
            {
                {1, 1}
            };
            var removed = tree.Remove(1);
            Assert.True(removed);
            Assert.Empty(tree);
            Assert.Null(tree.First);
            Assert.Null(tree.Last);
        }

        [Fact]
        public void KeyCollectionEnumeratorTest()
        {
            var tree = new AvlTreeList<int, int>();
            AddValues(tree);
            Assert.True(_sortedValues.SequenceEqual(tree.Keys));
        }

        [Fact]
        public void ValueCollectionEnumeratorTest()
        {
            var tree = new AvlTreeList<int, int>();
            AddValues(tree);
            Assert.True(_sortedValues.Select(i => i * 2).SequenceEqual(tree.Values));
        }

        private void AddValues(AvlTreeList<int, int> tree)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                var number = _values[i];
                tree.Add(number, number * 2);
            }
        }
    }
}
