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
                            .Aggregate((a, b) => (a.x + b.x, a.y + b.y));
            var pointsSize = points.Count();
            return (sumX/pointsSize, sumY/pointsSize);
        }

        public static (double, double) closest((double x, double y) point, ImmutableArray<(double x, double y)> clusters)
            => clusters.Aggregate((dist: euclideanDist(point, clusters[0]), clusterCenter: clusters[0]), (acc, c) => {
                var dist2 = euclideanDist(point, c);
                return acc.dist < dist2 ? acc : (dist2, c);
                }).clusterCenter;

        private static double euclideanDist((double x, double y) point, (double x, double y) c)
            => Math.Sqrt((point.x - c.x) * (point.x - c.x) + (point.y - c.y) * (point.y - c.y));

        static void Main(string[] args)
        {
            var points = System.IO.File.ReadAllLines("benchmarks/kmeans_concurrent/points.txt")
                .Select(x => (Convert.ToDouble(x.Split(':')[0]), Convert.ToDouble(x.Split(':')[1])))
                .ToImmutableArray();
            var clusters = runKMeans(10, points);
            clusters.ToImmutableList().ForEach(c => System.Console.WriteLine(c));
        }

        private static ImmutableArray<(double x, double y)> runKMeans(int clusterCount, ImmutableArray<(double x, double y)> points)
        {
            var clusters = points.Take(clusterCount).ToImmutableArray();
            return converge(clusters, points);
        }

        private static ImmutableArray<(double, double)> converge(ImmutableArray<(double x, double y)> clusters, ImmutableArray<(double x, double y)> points)
        {
            var newClusters = points
                        .GroupBy(p => closest(p, clusters))
                        .Select(g => computMean(g))
                        .ToImmutableArray();

            if(clusters.SequenceEqual(newClusters))
                return clusters;
            else
                return converge(newClusters, points);
        }
    }
}
