using System;
using System.Collections.Generic;
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
            var tree = new AvlTreeList<int>();
            Assert.Empty(tree);
            Assert.Null(tree.First);
            Assert.Null(tree.Last);
        }

        [Fact]
        public void TestListWithSingleItem()
        {
            var tree = new AvlTreeList<int>();
            var addedNode = tree.Add(1);
            Assert.NotNull(addedNode);
            _ = Assert.Single(tree);
            Assert.NotNull(tree.First);
            Assert.NotNull(tree.Last);
            Assert.Same(tree.First, tree.Last);
            Assert.Same(addedNode, tree.Last);
        }

        [Fact]
        public void RandomInsertTest()
        {
            var tree = new AvlTreeList<int>();
            AddValues(tree);
            Assert.Equal(ItemCount, tree.Count);
            Assert.Equal(_sortedValues[0], tree.First.Item);
            Assert.Equal(_sortedValues[ItemCount - 1], tree.Last.Item);
            Assert.Equal(_sortedValues[1], tree.First.Next.Item);
            Assert.Equal(_sortedValues[ItemCount - 2], tree.Last.Prev.Item);
            Assert.Equal(_sortedValues[2], tree.First.Next.Next.Item);
            Assert.Equal(_sortedValues[ItemCount - 3], tree.Last.Prev.Prev.Item);
        }

        [Fact]
        public void RemoveSingleItem()
        {
            var tree = new AvlTreeList<int>() { 1 };
            var removed = tree.Remove(1);
            Assert.True(removed);
            Assert.Empty(tree);
            Assert.Null(tree.First);
            Assert.Null(tree.Last);
        }

        private void AddValues(AvlTreeList<int> tree)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                var number = _values[i];
                tree.Add(number);
            }
        }
    }
}
