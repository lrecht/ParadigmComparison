using System;
using System.Collections.Generic;
using System.IO;


namespace procedural_c_
{
    class Program
    {
        static Dictionary<string, List<(string, int)>> edgeMap = new Dictionary<string, List<(string, int)>>();
        static void Main(string[] args)
        {

            string[] file = File.ReadAllLines("iotest/dijkstra/graph.csv");

            foreach (string edge in file)
            {
                string[] line = edge.Split(",");
                string from = line[0], to = line[1];
                int weight = Convert.ToInt32(line[2]);
                if (edgeMap.ContainsKey(from))
                    edgeMap[from].Add((to, weight));
                else
                    edgeMap.Add(from, new List<(string, int)> { (to, weight) });
            }
            System.Console.WriteLine(edgeMap.Count);
        }
    }
}