using System;
using System.Diagnostics;

namespace benchmark
{
    public class Benchmark
    {
        Stopwatch sw = new Stopwatch();
        float elapsedTime;
        static readonly string outputFilePath = "tempResults.csv";
        int iterations { get; }

        public Benchmark(int iterations) 
        {
            this.iterations = iterations;
        }

        public void Start() 
        {
            sw.Reset();
            sw.Start();
        }

        public void End()
        {
            sw.Stop();
            elapsedTime = sw.ElapsedMilliseconds;
        }

        public void Run<I, R>(Func<I, R> benchmark, I input, Action<R> print) 
        {
            for (int i = 0; i < iterations; i++)
            {
                Start();
                R res = benchmark(input);
                End();
                SaveResults();
                print(res);
            }
        }

        public void Run<R>(Func<R> benchmark, Action<R> print) 
        {
            for (int i = 0; i < iterations; i++)
            {
                Start();
                R res = benchmark();
                End();
                SaveResults();
                print(res);
            }
        }

        public void SaveResults()
        {
            System.IO.File.AppendAllText(outputFilePath, elapsedTime + "\n");
        }
    }
}
