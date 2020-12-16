using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkInterface;
using ShellProgressBar;

namespace newrun
{
    public static class BenchmarkRunner
    {
        static int SAMPLE_ITERATIONS = 100;
        public static void ExecuteBenchmarks(List<IBenchmark> benchmarkPrograms, long timeLimitSeconds)
        {
            BenchmarkStats statsObject = new BenchmarkStats();
            int numBenchmarks = benchmarkPrograms.Count;
            using (var pbar = new ProgressBar(numBenchmarks, "", new ProgressBarOptions { ProgressBarOnBottom = true, ShowEstimatedDuration = true }))
            {
                for (int i = 0; i < numBenchmarks; i++)
                {
                    string benchmarkName = benchmarkPrograms[i].GetType().FullName;
                    pbar.Tick($"Benchmark {i + 1} of {numBenchmarks}: {benchmarkName}");
                    // Run random samlpe
                    statsObject.Clear();
                    statsObject = executeSingleBenchmark(benchmarkPrograms[i], statsObject, SAMPLE_ITERATIONS, pbar, timeLimitSeconds);

                    // Compute number of samples needed for 0.5 % error
                    int numSignificantSamples = SampleSize.ComputeCochran(statsObject);
                    if (!(numSignificantSamples <= SAMPLE_ITERATIONS))
                    {
                        int remainingRuns = numSignificantSamples - SAMPLE_ITERATIONS;
                        statsObject = executeSingleBenchmark(benchmarkPrograms[i], statsObject, remainingRuns, pbar, timeLimitSeconds);
                    }
                    statsObject.SaveResults(benchmarkName);
                }
            }
        }

        static BenchmarkStats executeSingleBenchmark(IBenchmark currentBenchmark, BenchmarkStats stats, int iterations, ProgressBar pbar, long timeLimitSeconds = 0)
        {
            // Do nessecary preprocessing
            currentBenchmark.Preprocess();

            Stopwatch timeLimitWatch = new Stopwatch();
            Stopwatch currentBenchmarkWatch = new Stopwatch();
            timeLimitWatch.Start();
            using (var pbarChild = pbar.Spawn(iterations, "", new ProgressBarOptions { ProgressCharacter = '─', ProgressBarOnBottom = true }))
            {
                for (int i = 0; i < iterations; i++)
                {
                    // Advance progressbar
                    pbarChild.Tick($"Sample {i + 1} of {iterations}");

                    // Run benchmark
                    currentBenchmarkWatch.Start();
                    var result = currentBenchmark.Run();
                    currentBenchmarkWatch.Stop();

                    // Add results for each performance metric to statsobject
                    stats.AddResult(new double[] { currentBenchmarkWatch.ElapsedMilliseconds, 0, 0 }); // I don't have measures for power yet

                    // Break on time limit
                    if (timeLimitSeconds != 0 && timeLimitWatch.ElapsedMilliseconds / 1000 >= timeLimitSeconds)
                    {
                        System.Console.WriteLine("\nEnding benchmark due to time constraints");
                        timeLimitWatch.Stop();
                        break;
                    }
                    currentBenchmarkWatch.Reset();
                }
            }
            return stats;
        }
    }
}