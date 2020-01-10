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
    }
}
