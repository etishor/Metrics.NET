
using System;
namespace Metrics.StupidBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Benchmark.Run();

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
