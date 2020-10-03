using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace functional_c_
{
    class Program
    {

        static ImmutableList<(string, string, int)> EDGES = ImmutableList.Create<(string, string, int)>(
            //(source_vertex, dest_vertex, distance)
            ("a", "b", 7),
            ("a", "c", 9),
            ("a", "f", 14),
            ("b", "c", 10),
            ("b", "d", 15),
            ("c", "d", 11),
            ("c", "f", 2),
            ("d", "e", 6),
            ("e", "f", 9)
        );
        static readonly string START = "a";
        static readonly string END = "e";

        static void Main(string[] args)
        {
            //a, c, d, e
            var shortestPath = dijkstra(EDGES, START, END);
        }

        private static ImmutableList<string> dijkstra(ImmutableList<(string, string, int)> graph, string source, string destination) {
            if(source == destination)
                return ImmutableList.Create<string>(source);
            else{
                var vertices = graph.Select(x => x.Item1).Union(graph.Select(y => y.Item2)).ToImmutableList<string>();

                //(vertex, total_cost_to_get_to_vertex)
                var notVisited = vertices
                            .Where(x => x != source)
                            .Select(x => (x, int.MaxValue))
                            .Append((source, 0))
                            .ToImmutableSortedSet(Comparer<(string, int)>.Create((x, y) => x.Item2 > y.Item2 ? 1 : x.Item2 < y.Item2 ? -1 : string.Compare(x.Item1, y.Item1)));

                return dijkstraHelper(graph, source, destination, notVisited);
            }
        }

        private static ImmutableList<string> dijkstraHelper(ImmutableList<(string, string, int)> graph, string source, string destination, ImmutableSortedSet<(string, int)> notVisited) {
            var vertex = notVisited.First();

            if(vertex.Item1 == destination){
                System.Console.WriteLine("Reached final vertex " + vertex.Item1 + " with cost " + vertex.Item2);
                return null; //TODO: Add backtrack
            }
            else {
                return dijkstraHelper(graph, 
                                    source, 
                                    destination, 
                                    updateCosts(graph, 
                                                vertex, 
                                                notVisited.Remove(vertex)));
            }
        }

        private static ImmutableSortedSet<(string, int)> updateCosts(ImmutableList<(string, string, int)> graph, (string, int) currentVertex, ImmutableSortedSet<(string, int)> notVisited)
        {
            return notVisited
                .Select(x => {
                    var edge = graph.Find(y => y.Item1 == currentVertex.Item1 && y.Item2 == x.Item1);
                    
                    //If an edge from current to unvisited does not exist, return
                    if(!(edge.Item1 == currentVertex.Item1 && edge.Item2 == x.Item1))
                        return x;

                    var alternateCost = currentVertex.Item2 + edge.Item3;
                    if(x.Item2 > alternateCost)
                        return (x.Item1, alternateCost);
                    else
                        return x;
                })
                .ToImmutableSortedSet(notVisited.KeyComparer);
        }
    }
}
