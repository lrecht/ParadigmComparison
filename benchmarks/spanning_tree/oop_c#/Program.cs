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
            Edge[] edges = File.ReadAllLines($"benchmarks/spanning_tree/graph.csv")

                                           .Select(v => Edge.FromCsv(v, i++))
                                           .ToArray();
            Graph graph = new Graph(edges, 5877);
            (int totalweight, int totaledges) = graph.ComputeSpanningTree();
            System.Console.WriteLine(totaledges);
            System.Console.WriteLine(totalweight);
        }
    }

    public interface ISpanningTree
    {
        (int, int) ComputeSpanningTree();
    }

    public class Graph : ISpanningTree
    {
        SortedSet<Edge> edges; // collection of all edges, sorted by weight
        private int vertexCount, totalWeight, totalEdges;

        public Graph(Edge[] edges, int vertexCount)
        {
            this.vertexCount = vertexCount;
            this.edges = new SortedSet<Edge>(edges);
        }

        public (int, int) ComputeSpanningTree()
        {
            List<Edge> res = new List<Edge>();
            UnionFind uf = new UnionFind(vertexCount);
            while (res.Count < vertexCount - 1)
            {
                Edge currentEdge = edges.Min;
                edges.Remove(edges.Min);
                if (uf.Union(currentEdge.Node1, currentEdge.Node2))
                {
                    res.Add(currentEdge);
                    totalWeight += currentEdge.Weight;
                    totalEdges++;
                }
            }
            return (totalWeight, totalEdges);
        }
    }

    public class Edge : IComparable<Edge>
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

        public override bool Equals(object obj)
        {
            Edge other = (Edge)obj;
            return this.Node1.Equals(other.Node1) && this.Node2.Equals(other.Node2);
        }

        public int CompareTo(Edge other)
        {
            if (Weight == other.Weight)
                return id.CompareTo(other.id);
            return Weight.CompareTo(other.Weight);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString() =>
            $"{Node1}, {Node2}, with weight {Weight}";
    }

    public class UnionFind
    {
        private int[] vertexGroups;

        public UnionFind(int totalNodes)
        {
            vertexGroups = new int[6005 + 1];
            //vertexGroups = new int[totalNodes + 1];
            Array.Fill(vertexGroups, -1); // No verticies belong to a group
        }


        // find root and make root as parent of i (path compression)
        public int Find(int node)
        {
            if (vertexGroups[node] < 0) // Not already in a group (becomes it own)
                return node;
            else // If already in a group, we want to find the root of that group
            {
                vertexGroups[node] = Find(vertexGroups[node]);
                return vertexGroups[node];
            }
        }

        public bool Union(int node1, int node2)
        {
            int group1Root = Find(node1);
            int group2Root = Find(node2);

            // If both nodes belong to the same group, they create a cycle
            if (group1Root == group2Root) 
                return false;

            // If they belong to different groups, we merge the groups.
            vertexGroups[group2Root] = group1Root;
            return true;
        }
    }
}

//58436
//5876
