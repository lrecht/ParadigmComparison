using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BenchmarkInterface;

namespace newrun
{
    public static class BenchmarkUtils
    {

        public static List<IBenchmark> GetBenchmarkClasses(string[] namespaces)
        {
            var assemblies = LoadAssemblies(namespaces);
            var type = typeof(IBenchmark);
            var allClasses = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
                .Select(x => (IBenchmark)Activator.CreateInstance(x, null))
                .ToList();
            return allClasses;
        }
        static List<Assembly> LoadAssemblies(string[] namespaces)
        {
            var assemblies = new List<Assembly>();
            foreach (string asm in namespaces)
                assemblies.Add(Assembly.Load(asm));
            return assemblies;
        }
    }
}