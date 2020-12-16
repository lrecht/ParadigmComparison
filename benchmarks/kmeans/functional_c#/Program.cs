using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using benchmark;

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

        public static (double, double) closest((double x, double y) point, ImmutableList<(double x, double y)> clusters)
            => clusters.Aggregate((euclideanDist(point, clusters.First()), clusters.First()), (acc, c) => {
                var dist2 = euclideanDist(point, c);
                return acc.Item1 < dist2 ? acc : (dist2, c);
                }).Item2;

        private static double euclideanDist((double x, double y) point, (double x, double y) c)
            => Math.Sqrt(Math.Pow(point.x - c.x, 2) + Math.Pow(point.y - c.y, 2));

        static void Main(string[] args)
        {
            var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
            var bm = new Benchmark(iterations);

			var points = System.IO.File.ReadAllLines("benchmarks/kmeans_concurrent/points.txt")
                .Select(x => (Convert.ToDouble(x.Split(':')[0]), Convert.ToDouble(x.Split(':')[1])))
                .ToImmutableList();

			bm.Run(() => {
            	return runKMeans(10, points);
			}, (res) => {
				res.ForEach(c => System.Console.WriteLine(c));
			});
        }

        private static ImmutableList<(double x, double y)> runKMeans(int clusterCount, ImmutableList<(double x, double y)> points)
        {
            var clusters = points.Take(clusterCount).ToImmutableList();
            return converge(clusters, points);
        }

        private static ImmutableList<(double, double)> converge(ImmutableList<(double x, double y)> clusters, ImmutableList<(double x, double y)> points)
        {
            var newClusters = points
                        .GroupBy(p => closest(p, clusters))
                        .Select(g => computMean(g))
                        .ToImmutableList();

            if(clusters.SequenceEqual(newClusters))
                return clusters;
            else
                return converge(newClusters, points);
        }
    }
}
