using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using benchmark;

namespace functional_c_
{
    class Program
    {
        static Comparer<(string, int, string)> vertexComparer = Comparer<(string, int, string)>.Create((x, y) => x.Item2 > y.Item2 ? 1 : x.Item2 < y.Item2 ? -1 : string.Compare(x.Item1, y.Item1));
        static void Main(string[] args)
        {
            var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
            var bm = new Benchmark(iterations);
            
			var file = System.IO.File.ReadAllLines("benchmarks/dijkstra/graph.csv");
            
			bm.Run(() => {
            	ImmutableArray<(string, string, int)> EDGES = getEdgesFromCsv(file);
                string START = "257";
                string END = "5525";

                ImmutableDictionary<string, ImmutableArray<(string, string, int)>> graph = getGraphFromEdges(EDGES);
                return dijkstra(graph, START, END);
            }, (res) => {
                System.Console.WriteLine(string.Join(' ', res));
            });
        }

        private static ImmutableArray<(string from, string to, int cost)> getEdgesFromCsv(string[] file)
        {
            return file
                .Select(line =>
                {
                    var values = line.Split(',');
                    var from = System.Convert.ToString(values[0]);
                    var to = System.Convert.ToString(values[1]);
                    var cost = System.Convert.ToInt32(values[2]);
                    return (from, to, cost);
                })
                .ToImmutableArray();
        }

        private static ImmutableDictionary<string, ImmutableArray<(string from, string to, int cost)>> getGraphFromEdges(ImmutableArray<(string from, string to, int cost)> edges)
        {
            return edges
                .GroupBy(edge => edge.from)
                .Select(grouping => grouping.ToImmutableArray())
                .ToImmutableDictionary(x => x.First().from);
        }

        private static ImmutableArray<string> backtrack(ImmutableDictionary<string, (string, int, string)> visited, string source, (string, int, string) vertex, ImmutableArray<string> path)
        {
            if (vertex.Item3 == source)
                return path.Add(vertex.Item1).Add(source);

            return backtrack(
                visited,
                source,
                visited[vertex.Item3],
                path.Add(vertex.Item1)
            );
        }

        private static ImmutableArray<string> dijkstra(ImmutableDictionary<string, ImmutableArray<(string from, string to, int cost)>> graph, string source, string destination)
        {
            if (source == destination)
                return ImmutableArray.Create<string>(source);
            else
            {
                //(vertex, total_cost_to_get_to_vertex, previous_vertex)
                var vertices = graph
                            .SelectMany(x => x.Value
                            .Select(y => y.to))
                            .Union(graph.Select(x => x.Key))
                            .Where(x => x != source)
                            .Select(x => (x, int.MaxValue, ""))
                            .Append((source, 0, ""));

                var vertexQueue = vertices
                            .ToImmutableSortedSet(vertexComparer);

                var vertexCosts = vertices
                            .ToImmutableDictionary(x => x.Item1);

                var visited = ImmutableDictionary<string, (string, int, string)>.Empty;

                return dijkstraHelper(graph, source, destination, vertexQueue, visited, vertexCosts);
            }
        }

        private static ImmutableArray<string> dijkstraHelper(
            ImmutableDictionary<string, ImmutableArray<(string from, string to, int cost)>> graph, 
            string source, 
            string destination, 
            ImmutableSortedSet<(string name, int costToReach, string previous)> vertexQueue, 
            ImmutableDictionary<string, (string, int, string)> visited, 
            ImmutableDictionary<string, (string, int, string)> vertexCosts
            )
        {
            var vertex = vertexQueue.Min;
            if(visited.ContainsKey(vertex.name))
                return dijkstraHelper(graph, source, destination, vertexQueue.Remove(vertex), visited, vertexCosts);

            var newVisited = visited.Add(vertex.name, vertex);

            if (vertex.name == destination)
            {
                return backtrack(newVisited, source, newVisited[destination], ImmutableArray<string>.Empty)
                        .Reverse()
                        .ToImmutableArray();
            }
            else
            {
                var (newNotVisited, newCosts) = updateCosts(graph, vertex, vertexQueue.Remove(vertex), newVisited, vertexCosts);
                return dijkstraHelper(graph,
                                    source,
                                    destination,
                                    newNotVisited,
                                    newVisited,
                                    newCosts);
            }
        }

        private static (ImmutableSortedSet<(string, int, string)>, ImmutableDictionary<string, (string, int, string)>) updateCosts(
            ImmutableDictionary<string, ImmutableArray<(string source, string dest, int cost)>> graph, 
            (string name, int costToReach, string previous) currentVertex, 
            ImmutableSortedSet<(string, int, string)> vertexQueue, 
            ImmutableDictionary<string, (string, int, string)> visited, 
            ImmutableDictionary<string, (string name, int costToReach, string previous)> vertexCosts
            )
        {
            if (!graph.ContainsKey(currentVertex.name))
                return (vertexQueue, vertexCosts);

            var cheaperVertices = graph[currentVertex.name]
                .Where(edge => !visited.ContainsKey(edge.dest))
                .Where(edge =>
                {
                    var vertex = vertexCosts[edge.dest];

                    var alternateCost = currentVertex.costToReach + edge.cost;
                    return alternateCost < vertex.costToReach;
                })
                .Select(edge => {
                    var vertex = vertexCosts[edge.dest];
                    var alternateCost = currentVertex.costToReach + edge.cost;
                    return (vertex.name, alternateCost, currentVertex.name);
                });

            var newCosts = vertexCosts
                .SetItems(cheaperVertices.Select(x => KeyValuePair.Create<string, (string, int, string)>(x.Item1, x)));

            var newQueue = vertexQueue
                .Union(cheaperVertices)
                .ToImmutableSortedSet(vertexComparer);

            return (newQueue, newCosts);
        }
    }
}