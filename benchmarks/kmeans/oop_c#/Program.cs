using System;
using System.IO;
using System.Linq;
using benchmark;

namespace oop_c_
{
	class Program
	{
		private static int NUM_CLUSTERS = 10;
		private static Point[] points;
		private static Cluster[] clusters;
		static void Main(string[] args)
		{
			var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations, silenceBenchmarkOutput: true);

			var file = File.ReadAllLines($"benchmarks/kmeans/points.txt");

			bm.Run(() =>
			{
				points = file.Select(l => new Point(Convert.ToDouble(l.Split(':')[0]), Convert.ToDouble(l.Split(':')[1])))
								.ToArray();
				clusters = new Cluster[NUM_CLUSTERS];
				for (int i = 0; i < NUM_CLUSTERS; i++)
					clusters[i] = new Cluster(points[i]);

				KMeans kmeans = new KMeans(points, clusters);
				return kmeans.Compute();
			}, (res) =>
			{
				foreach (Cluster c in res)
					System.Console.WriteLine(c.Centroid);
			});
		}
	}

	public class KMeans
	{
		Point[] initialPoints { get; set; }
		Cluster[] clusters { get; set; }
		public KMeans(Point[] points, Cluster[] kMeans)
		{
			initialPoints = points;
			clusters = kMeans;
		}

		public Cluster[] Compute()
		{
			bool converged = false;
			while (!converged)
			{
				// Assignment step: put each point in exactly one cluster
				foreach (Point p in initialPoints)
					p.ClosestCluster(clusters).AddToMean(p);

				// Update step: recompute mean of each cluster
				converged = true;
				foreach (Cluster c in clusters)
					converged &= c.ComputeNewCentroid();
			}
			return clusters;
		}
	}

	public class Point
	{
		public readonly double X, Y;
		public Point(double x, double y) => (X, Y) = (x, y);
		public Cluster ClosestCluster(Cluster[] clusters)
		{
			Cluster bestCluster = null;
			double bestDist = Double.PositiveInfinity, dist;
			foreach (Cluster c in clusters)
			{
				dist = euclidianDist(c.Centroid);
				if (dist < bestDist)
				{
					bestCluster = c;
					bestDist = dist;
				}
			}
			return bestCluster;
		}
		private double euclidianDist(Point centroid) => Math.Sqrt(Math.Pow((centroid.X - X), 2) + Math.Pow((centroid.Y - Y), 2));

		public override bool Equals(object obj)
		{
			Point other = (Point)obj;
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode();

		public override string ToString() => $"({X},{Y})";
	}

	public class Cluster
	{
		public Point Centroid { get; set; }
		double sumx, sumy;
		int pointCount;
		public Cluster(Point initailMean)
		{
			Centroid = initailMean;
		}

		public void AddToMean(Point p)
		{
			sumx += p.X;
			sumy += p.Y;
			pointCount++;
		}

		public bool ComputeNewCentroid()
		{
			Point newCentroid = new Point(sumx / pointCount, sumy / pointCount);
			bool isConverged = Centroid.Equals(newCentroid);
			Centroid = newCentroid;
			clearMeanValues();
			return isConverged;
		}

		private void clearMeanValues()
		{
			sumx = 0.0;
			sumy = 0.0;
			pointCount = 0;
		}
	}
}
