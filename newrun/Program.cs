using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BenchmarkInterface;

namespace newrun
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] assemblies = new string[] {"CBenchmarks"}; 
            List<IBenchmark> allBenchmarks = BenchmarkUtils.GetBenchmarkClasses(assemblies);
            allBenchmarks = allBenchmarks.Where(x => x.GetType().FullName.Contains("Procedural")).ToList();
            BenchmarkRunner.ExecuteBenchmarks(allBenchmarks, 60);
        }
    }
}

