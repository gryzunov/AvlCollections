using Xunit;

namespace DataStructures.Tests
{
    public class AvlTreeListTests
    {
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
            var tree = new AvlTreeList<int, int>
            {
                { 15, 15 },
                { 7, 7 },
                { 6, 6 },
                { 25, 25 },
                { 20, 20 }
            };
            Assert.Equal(5, tree.Count);
            Assert.Equal(6, tree.First.Key);
            Assert.Equal(25, tree.Last.Key);
            Assert.Equal(7, tree.First.Next.Key);
            Assert.Equal(20, tree.Last.Prev.Key);
            Assert.Equal(15, tree.First.Next.Next.Key);
            Assert.Equal(15, tree.Last.Prev.Prev.Key);
        }
    }
}
