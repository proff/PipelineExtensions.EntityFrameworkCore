using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace PipelineExtensions.EntityFrameworkCore.PerfTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            BenchmarkRunner.Run<Test>();
            /*var test = new Test();
            test.GlobalSetup();
            for (int i = 0; i < 100000; i++)
                await test.WithPipeline_NewContext_ToArrayAsync();
            Console.WriteLine("ready");
            Console.ReadLine();
            for (int i = 0; i < 100000; i++)
                await test.WithPipeline_NewContext_ToArrayAsync();
            Console.WriteLine("done");
            Console.ReadLine();*/
        }
    }
}