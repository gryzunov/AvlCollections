using Xunit;

namespace DataStructures.Tests
{
    public class AvlTree2Tests
    {
        [Fact]
        public void TestInsert()
        {
            var tree = new AvlTree2<int>();
            tree.Add(30);
            tree.Add(10);
            tree.Add(40);
            tree.Add(20);
            tree.Add(50);
        }
    }
}
