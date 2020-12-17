using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace functional_c_
{
    class Program
    {
        static readonly int magicVertexCount = 5877;
        static void Main(string[] args)
        {
            var edgesRep = System.IO.File.ReadAllLines("benchmarks/spanning_tree/graph.csv")
                            .Select(l => l.Split(','))
                            .Select((val, i) => (i, Convert.ToInt32(val[0]), Convert.ToInt32(val[1]), Convert.ToInt32(val[2])));

            var edges = edgesRep.OrderBy(e => e.Item4).ToImmutableArray();
            var edgesDict = edgesRep.ToImmutableDictionary(e => e.i);

            var spanningTreeEdges = getMinimumSpanningTree(edges);
            var resultEdges = spanningTreeEdges.Select(i => edgesDict[i]).ToImmutableArray();

            var totalweight = resultEdges.Sum(e => e.Item4);
            var totalEdges = resultEdges.Length;
            
            System.Console.WriteLine(totalEdges);
            System.Console.WriteLine(totalweight);
        }

        static ImmutableList<int> getMinimumSpanningTree(ImmutableArray<(int, int, int, int)> edges)
            => getMinimumSpanningTreeHelper(edges, 0, ImmutableList<int>.Empty, ImmutableDictionary<int, int>.Empty);

        static ImmutableList<int> getMinimumSpanningTreeHelper(ImmutableArray<(int i, int v1, int v2, int)> edges, int i, ImmutableList<int> spanningTree, ImmutableDictionary<int, int> rootDict){
            if(spanningTree.Count >= magicVertexCount - 1) //-1 as one less edge than vertex is required to complete graph
                return spanningTree;

            var currentEdge = edges[i];
            var (res, updatedDict) = union(currentEdge.v1, currentEdge.v2, rootDict);

            if(res)
                return getMinimumSpanningTreeHelper(edges, i + 1, spanningTree.Add(currentEdge.i), updatedDict);
            else
                return getMinimumSpanningTreeHelper(edges, i + 1, spanningTree, updatedDict);
        }

        private static (bool, ImmutableDictionary<int, int>) union(int v1, int v2, ImmutableDictionary<int, int> rootDict)
        {
            var group1Root = find(v1, rootDict);
            var group2Root = find(v2, rootDict);
            
            var update1 = new KeyValuePair<int, int>(v1, group1Root);
            var update2 = new KeyValuePair<int, int>(v2, group2Root);

            var updatedDict = rootDict.SetItems(new[] {update1, update2});

            if(group1Root == group2Root)
                return (false, updatedDict);
            else
                return (true, updatedDict.SetItem(group2Root, group1Root));
        }

        private static int find(int vertex, ImmutableDictionary<int, int> rootDict)
            => rootDict.ContainsKey(vertex) && rootDict[vertex] != vertex ? find(rootDict[vertex], rootDict) : vertex;
    }
}
