using BenchmarkDotNet.Running;

namespace DataStructures.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<AvlTreeVsRBTreeInsertRandom>();
            var summary = BenchmarkRunner.Run<AvlVariantsCompare>();
        }
    }
}
