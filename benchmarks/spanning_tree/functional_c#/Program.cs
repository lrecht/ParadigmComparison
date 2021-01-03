using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using benchmark;

namespace functional_c_
{
    class Program
    {
        static readonly int magicVertexCount = 5877;
        static void Main(string[] args)
        {
            var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations, silenceBenchmarkOutput: true);

			var file = System.IO.File.ReadAllLines("benchmarks/spanning_tree/graph.csv");
			bm.Run(() => {
				var edgesRep = file
								.Select(l => l.Split(','))
								.Select((val, i) => (i, Convert.ToInt32(val[0]), Convert.ToInt32(val[1]), Convert.ToInt32(val[2])));

				var edges = edgesRep.OrderBy(e => e.Item4).ToImmutableArray();
				var edgesDict = edgesRep.ToImmutableDictionary(e => e.i);

				var spanningTreeEdges = getMinimumSpanningTree(edges);
				var resultEdges = spanningTreeEdges.Select(i => edgesDict[i]).ToImmutableArray();

				var totalweight = resultEdges.Sum(e => e.Item4);
				var totalEdges = resultEdges.Length;
				return (totalEdges, totalweight);
			}, (res) => {
				System.Console.WriteLine(res.Item1);
				System.Console.WriteLine(res.Item2);
			});
        }

        static ImmutableList<int> getMinimumSpanningTree(ImmutableArray<(int, int, int, int)> edges)
            => getMinimumSpanningTreeHelper(edges, 0, new List<int>(), new Dictionary<int, int>(edges.Select(e => new KeyValuePair<int, int>(e.Item1, e.Item1))));

        static ImmutableList<int> getMinimumSpanningTreeHelper(ImmutableArray<(int i, int v1, int v2, int)> edges, int i, List<int> spanningTree, Dictionary<int, int> rootDict){
            if(spanningTree.Count >= magicVertexCount - 1) //-1 as one less edge than vertex is required to complete graph
                return spanningTree.ToImmutableList();

            var currentEdge = edges[i];
            var res = union(currentEdge.v1, currentEdge.v2, rootDict);

            if(res)
                spanningTree.Add(currentEdge.i);
                    
            return getMinimumSpanningTreeHelper(edges, i + 1, spanningTree, rootDict);
        }

        private static bool union(int v1, int v2, Dictionary<int, int> rootDict)
        {
            var group1Root = find(v1, rootDict);
            var group2Root = find(v2, rootDict);
            
            rootDict[v1] = group1Root;
            rootDict[v2] = group2Root;

            if(group1Root == group2Root)
                return false;
            else{
                rootDict[group2Root] = group1Root;
                return true;
            }
                
        }

        private static int find(int vertex, Dictionary<int, int> rootDict)
            => rootDict.ContainsKey(vertex) && rootDict[vertex] != vertex ? find(rootDict[vertex], rootDict) : vertex;
    }
}
