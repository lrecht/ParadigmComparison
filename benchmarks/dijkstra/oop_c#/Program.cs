using System;
using System.Collections.Generic;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        static readonly List<Edge> EDGES = new List<Edge>
        {
            new Edge("a", "b", 7),
            new Edge("a", "c", 9),
            new Edge("a", "f", 14),
            new Edge("b", "c", 10),
            new Edge("b", "d", 15),
            new Edge("c", "d", 11),
            new Edge("c", "f", 2),
            new Edge("d", "e", 6),
            new Edge("e", "f", 9)
        };
        static readonly string START = "a";
        static readonly string END = "e";
        static void Main(string[] args)
        {
            Graph graph = new Graph(EDGES);
            var shortestPath = graph.dijkstra(START, END);
            System.Console.WriteLine(String.Join(' ', shortestPath));
        }
    }

    public class Graph
    {
        List<Edge> _edges { get; set; }
        // mapping of vertex names to Vertex objects, built from a set of Edges
        readonly Dictionary<string, Vertex> graph = new Dictionary<string, Vertex>();
        public Graph(List<Edge> edges)
        {
            _edges = edges;
            // Adds vertices to graph
            foreach (Edge e in _edges)
            {
                if (!graph.ContainsKey(e.start))
                    graph.Add(e.start, new Vertex(e.start));
                if (!graph.ContainsKey(e.end))
                    graph.Add(e.end, new Vertex(e.end));
            }

            // Adds neighbours to vertices
            foreach (Edge edge in _edges)
                graph[edge.start].neighbours.Add(graph[edge.end], edge.cost);
        }

        public List<string> dijkstra(string start, string end)
        {
            if (!graph.ContainsKey(start))
                throw new Exception("Graph doesn't contain start vertex");

            Vertex source = graph[start];
            source.dist = 0;
            Vertex dest = graph[end];
            SortedSet<Vertex> vertex_queue = new SortedSet<Vertex>() { source };
            Vertex current, neighbour;
            while (vertex_queue.Count > 0)
            {
                current = vertex_queue.First();
                vertex_queue.Remove(current);
                if (current.Equals(dest))
                    break;

                foreach (var n in current.neighbours)
                {
                    neighbour = n.Key;
                    var alternateDist = current.dist + n.Value;
                    if (alternateDist < neighbour.dist)
                    {
                        vertex_queue.Remove(neighbour);
                        neighbour.dist = alternateDist;
                        neighbour.previous = current;
                        vertex_queue.Add(neighbour);
                    }
                }
            }

            List<string> shortestPath = new List<string>();
            Vertex previous = dest;
            while (previous != null)
            {
                shortestPath.Insert(0,previous.name);
                previous = previous.previous;
            }
            return shortestPath;
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
    }

    public class Vertex : IComparable
    {
        public readonly string name;
        public readonly Dictionary<Vertex, int> neighbours = new Dictionary<Vertex, int>();
        public int dist = int.MaxValue;
        public Vertex previous = null;
        public Vertex(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            Vertex other = (Vertex)obj;
            return this.name.Equals(other.name);
        }

        public int CompareTo(object obj)
        {
            Vertex other = (Vertex)obj;
            return dist.CompareTo(other.dist);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}
