using Xunit;
using Xunit.Abstractions;

namespace DataStructures.Tests
{
    public class CountingComparerTests
    {
        private readonly ITestOutputHelper _output;

        public CountingComparerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestComparer()
        {
            var comparer1 = new CountingComparer();
            var comparer2 = new CountingComparer();
            var comparer3 = new CountingComparer();
            var tree1 = new AvlTree<int>(comparer1);
            var tree2 = new CompactAvlTree<int>(comparer2);
            var tree3 = new CompactAvlTree2<int>(comparer3);

            for (int i = 1; i <= 10000; i++)
            {
                tree1.Add(i);
                tree2.Add(i);
                tree3.Add(i);
            }

            _output.WriteLine($"AvlTree compare calls per 10000 inserts: {comparer1.Count}");
            _output.WriteLine($"CompactAvlTree compare calls per 10000 inserts: {comparer2.Count}");
            _output.WriteLine($"CompactAvlTree2 compare calls per 10000 inserts: {comparer3.Count}");

            Assert.True(comparer1.Count > 0);
            Assert.True(comparer2.Count > 0);
            Assert.True(comparer3.Count > 0);
        }
    }
}
