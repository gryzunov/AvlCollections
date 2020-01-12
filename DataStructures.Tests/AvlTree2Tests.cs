using Xunit;

namespace DataStructures.Tests
{
    public class AvlTree2Tests
    {
        [Fact]
        public void TestInsert()
        {
            var tree = new AvlTree2<int>();
            tree.Insert(30);
            tree.Insert(10);
            tree.Insert(40);
            tree.Insert(20);
            tree.Insert(50);
        }
    }
}
