using Xunit;

namespace DataStructures.Tests
{
    public class CompactAvlTreeTests
    {
        [Fact]
        public void TestInsert()
        {
            var tree = new CompactAvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                Assert.True(tree.Add(i));
            }
        }

        [Fact]
        public void TestDelete()
        {
            var tree = new CompactAvlTree<int>();
            for (int i = 1; i <= 100; i++)
            {
                tree.Add(i);
            }
            Assert.False(tree.Remove(1000));
            for (int i = 1; i <= 100; i++)
            {
                Assert.True(tree.Remove(i));
            }
        }
    }
}
