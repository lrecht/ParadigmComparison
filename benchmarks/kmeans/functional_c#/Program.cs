using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace functional_c_
{
    class Program
    {
        public static (double x, double y) computMean(IEnumerable<(double x, double y)> points){
            var (sumX, sumY) = points
                            .Select(p => (p.x, p.y))
                            .Aggregate((a, b) => (a.x + b.x, a.y + b.y));
            var pointsSize = points.Count();
            return (sumX/pointsSize, sumY/pointsSize);
        }

        public static (double, double) closest((double x, double y) point, ImmutableList<(double x, double y)> clusters)
            => clusters.OrderBy(c => euclideanDist(point, c)).First();

        private static double euclideanDist((double x, double y) point, (double x, double y) c)
            => Math.Sqrt(Math.Pow(point.x - c.x, 2) + Math.Pow(point.y - c.y, 2));

        static void Main(string[] args)
        {
            var points = System.IO.File.ReadAllLines("benchmarks/kmeans/points.txt").Select(x => (double.Parse(x.Split(':')[0]), double.Parse(x.Split(':')[1]))).ToImmutableList();
            var clusters = runKMeans(10, points);
            clusters.ForEach(c => System.Console.WriteLine(c));
        }

        private static ImmutableList<(double x, double y)> runKMeans(int clusterCount, ImmutableList<(double x, double y)> points)
        {
            var clusters = points.Take(clusterCount).ToImmutableList();
            return converge(clusters, points);
        }

        private static ImmutableList<(double, double)> converge(ImmutableList<(double x, double y)> clusters, ImmutableList<(double x, double y)> points)
        {
            var groups = points
                        .GroupBy(p => closest(p, clusters));

            var newClusters = groups
                        .Select(g => computMean(g))
                        .ToImmutableList();

            if(clusters.SequenceEqual(newClusters))
                return clusters;
            else
                return converge(newClusters, points);
        }
    }
}
