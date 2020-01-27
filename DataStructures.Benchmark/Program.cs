using BenchmarkDotNet.Running;

namespace DataStructures.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //_ = BenchmarkRunner.Run<AvlTreeWalkCompare>();
            //_ = BenchmarkRunner.Run<AvlTreeVsRBTreeSearch>();
            _ = BenchmarkRunner.Run<AvlTreeVsRBTreeInsertRandom>();
            _ = BenchmarkRunner.Run<AvlTreeVsRBTreeInsertSorted>();
        }
    }
}
