﻿using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace functional_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            ImmutableList<(string, string, int)> EDGES = getEdgesFromCsv($"benchmarks/dijkstra/graph.csv");
            string START = "257";
            string END = "5525";

            ImmutableDictionary<string, ImmutableList<(string, string, int)>> graph = getGraphFromEdges(EDGES);
            var shortestPath = dijkstra(graph, START, END);
            System.Console.WriteLine(string.Join(' ', shortestPath));
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

        private static ImmutableDictionary<string, ImmutableList<(string, string, int)>> getGraphFromEdges(ImmutableList<(string, string, int)> edges)
        {
            return edges
                .GroupBy(edge => edge.Item1)
                .Select(grouping => grouping.ToImmutableList())
                .ToImmutableDictionary(x => x.First().Item1);
        }

        private static ImmutableList<string> backtrack(ImmutableDictionary<string, (string, int, string)> visited, string source, (string, int, string) vertex, ImmutableList<string> path)
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

        private static ImmutableList<string> dijkstra(ImmutableDictionary<string, ImmutableList<(string, string, int)>> graph, string source, string destination)
        {
            if (source == destination)
                return ImmutableList.Create<string>(source);
            else
            {
                var vertices = graph
                            .SelectMany(x => x.Value
                            .Select(y => y.Item2))
                            .Union(graph.Select(x => x.Key));

                //(vertex, total_cost_to_get_to_vertex, previous_vertex)
                var vertexQueue = vertices
                            .Where(x => x != source)
                            .Select(x => (x, int.MaxValue, ""))
                            .Append((source, 0, ""))
                            .ToImmutableSortedSet(Comparer<(string, int, string)>.Create((x, y) => x.Item2 > y.Item2 ? 1 : x.Item2 < y.Item2 ? -1 : string.Compare(x.Item1, y.Item1)));

                var vertexCosts = vertices
                            .Where(x => x != source)
                            .Select(x => (x, int.MaxValue, ""))
                            .Append((source, 0, ""))
                            .ToImmutableDictionary(x => x.Item1);

                var visited = ImmutableDictionary<string, (string, int, string)>.Empty;

                return dijkstraHelper(graph, source, destination, vertexQueue, visited, vertexCosts);
            }
        }

        private static ImmutableList<string> dijkstraHelper(ImmutableDictionary<string, ImmutableList<(string, string, int)>> graph, string source, string destination, ImmutableSortedSet<(string, int, string)> vertexQueue, ImmutableDictionary<string, (string, int, string)> visited, ImmutableDictionary<string, (string, int, string)> vertexCosts)
        {
            var vertex = vertexQueue.Min;
            if(visited.ContainsKey(vertex.Item1))
                return dijkstraHelper(graph, source, destination, vertexQueue.Remove(vertex), visited, vertexCosts);
                
            var newVisited = visited.Add(vertex.Item1, vertex);

            if (vertex.Item1 == destination)
            {
                return backtrack(newVisited, source, newVisited[destination], ImmutableList<string>.Empty).Reverse();
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

        private static (ImmutableSortedSet<(string, int, string)>, ImmutableDictionary<string, (string, int, string)>) updateCosts(ImmutableDictionary<string, ImmutableList<(string, string, int)>> graph, (string, int, string) currentVertex, ImmutableSortedSet<(string, int, string)> vertexQueue, ImmutableDictionary<string, (string, int, string)> visited, ImmutableDictionary<string, (string, int, string)> vertexCosts)
        {
            if (!graph.ContainsKey(currentVertex.Item1))
                return (vertexQueue, vertexCosts);

            var cheaperVertices = graph[currentVertex.Item1]
                .Where(edge => !visited.ContainsKey(edge.Item2))
                .Where(edge =>
                {
                    var vertex = vertexCosts[edge.Item2];

                    var alternateCost = currentVertex.Item2 + edge.Item3;
                    return alternateCost < vertex.Item2;
                })
                .Select(edge => {
                    var vertex = vertexCosts[edge.Item2];
                    var alternateCost = currentVertex.Item2 + edge.Item3;
                    return (vertex.Item1, alternateCost, currentVertex.Item1);
                });

            var newCosts = vertexCosts
                .SetItems(cheaperVertices.Select(x => KeyValuePair.Create<string, (string, int, string)>(x.Item1, x)));

            var newQueue = vertexQueue
                .Union(cheaperVertices)
                .ToImmutableSortedSet(vertexQueue.KeyComparer);

            return (newQueue, newCosts);
        }
    }
}