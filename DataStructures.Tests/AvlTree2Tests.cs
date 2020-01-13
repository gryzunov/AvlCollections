using Xunit;

namespace DataStructures.Tests
{
    public class AvlTree2Tests
    {
        [Fact]
        public void TestInsert()
        {
            var tree = new CompactAvlTree<int>();
            Assert.True(tree.Add(30));
            Assert.True(tree.Add(10));
            Assert.True(tree.Add(40));
            Assert.True(tree.Add(20));
            Assert.True(tree.Add(50));
        }

        [Fact]
        public void TestDelete()
        {
            var tree = new CompactAvlTree<int>();
            tree.Add(30);
            tree.Add(10);
            tree.Add(40);
            tree.Add(20);
            tree.Add(50);

            Assert.False(tree.Remove(100));
            Assert.True(tree.Remove(30));
            Assert.True(tree.Remove(10));
            Assert.True(tree.Remove(40));
            Assert.True(tree.Remove(20));
            Assert.True(tree.Remove(50));
        }
    }
}
