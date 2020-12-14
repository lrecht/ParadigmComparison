using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            string directory = System.IO.Directory.GetParent(Environment.CurrentDirectory).ToString();
            List<Edge> edges = File.ReadAllLines($"benchmarks/dijkstra/graph.csv")
                                           .Select(v => Edge.FromCsv(v))
                                           .ToList();
            System.Console.WriteLine(edges.Count);
        }
    }

    
    public class Edge
    {
        public string start, end;
        public int cost;
        public Edge(string start, string end, int cost)
        {
            this.start = start;
            this.end = end;
            this.cost = cost;
        }

        public static Edge FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            Edge edge = new Edge(Convert.ToString(values[0]), Convert.ToString(values[1]), Convert.ToInt32(values[2]));
            return edge;
        }
    }
}
