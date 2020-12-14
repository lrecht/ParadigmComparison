using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace functional_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            ImmutableList<(string, string, int)> EDGES = getEdgesFromCsv($"benchmarks/dijkstra/graph.csv");
            System.Console.WriteLine(string.Join(' ', EDGES.Count()));
        }

        private static ImmutableList<(string, string, int)> getEdgesFromCsv(string filePath)
        {
            return System.IO.File.ReadAllLines(filePath)
                .Select(line =>
                {
                    var values = line.Split(',');
                    var from = System.Convert.ToString(values[0]);
                    var to = System.Convert.ToString(values[1]);
                    var cost = System.Convert.ToInt32(values[2]);
                    return (from, to, cost);
                })
                .ToImmutableList();
        }
    }
}