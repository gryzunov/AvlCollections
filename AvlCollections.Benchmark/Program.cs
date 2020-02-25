using BenchmarkDotNet.Running;

namespace AvlCollections.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run(typeof(AvlTreeWalkBenchmark).Assembly);
        }
    }
}
