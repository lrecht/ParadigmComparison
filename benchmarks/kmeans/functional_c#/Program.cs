using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace functional_c_
{
    readonly struct Cluster {
        public Cluster((double x, double y) mean){
            this.mean = mean;
        }
        public readonly (double x, double y) mean;
    }

    class Program
    {
        public static (double x, double y) computMean(IEnumerable<(double x, double y)> points){
            var sumX = points.Select(p => p.x).Sum();
            var sumY = points.Select(p => p.y).Sum(); //TODO: Improve effciency by returning tuple with X AND Y
            var pointsSize = points.Count();
            return (sumX/pointsSize, sumY/pointsSize);
        }

        public static Cluster closest((double x, double y) point, ImmutableList<Cluster> clusters)
            => clusters.OrderBy(c => euclideanDist(point, c)).First();

        private static double euclideanDist((double x, double y) point, Cluster c)
            => Math.Sqrt(Math.Pow(point.x - c.mean.x, 2) + Math.Pow(point.y - c.mean.y, 2));

        static void Main(string[] args)
        {
            var points = System.IO.File.ReadAllLines("benchmarks/kmeans/points.txt").Select(x => (double.Parse(x.Split(':')[0]), double.Parse(x.Split(':')[1]))).ToImmutableList();
            var clusters = runKMeans(2, points);
            clusters.ForEach(c => System.Console.WriteLine(c.mean));
        }

        private static ImmutableList<Cluster> runKMeans(int clusterCount, ImmutableList<(double x, double y)> points)
        {
            var clusters = points.Take(clusterCount).Select(p => new Cluster(p)).ToImmutableList();
            return converge(clusters, points);
        }

        private static ImmutableList<Cluster> converge(ImmutableList<Cluster> clusters, ImmutableList<(double x, double y)> points)
        {
            var groups = points
                        .GroupBy(p => closest(p, clusters));

            var newClusters = groups
                        .Select(g => new Cluster(computMean(g)))
                        .ToImmutableList();

            if(clusters.All(c => newClusters.Any(nc => nc.mean == c.mean)))
                return clusters;
            else
                return converge(newClusters, points);
        }
    }
}
