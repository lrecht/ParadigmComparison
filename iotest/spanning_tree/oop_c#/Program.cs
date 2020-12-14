using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            Edge[] edges = File.ReadAllLines($"iotest/spanning_tree/graph.csv")
                                           .Select(v => Edge.FromCsv(v, i++))
                                           .ToArray();
            System.Console.WriteLine(edges.Length);
        }
    }
    public class Edge
    {
        public int Node1, Node2, Weight;
        int id;
        public Edge(int start, int end, int cost)
        {
            this.Node1 = start;
            this.Node2 = end;
            this.Weight = cost;
        }

        public static Edge FromCsv(string csvLine, int id)
        {
            string[] values = csvLine.Split(',');
            Edge edge = new Edge(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]), Convert.ToInt32(values[2]));
            edge.id = id;
            return edge;
        }
    }
}
