using System;
using System.Threading.Tasks;
using benchmark;

namespace procedural_c_
{
	class Program
	{
		static int numKlusters = 10;
		static Random rand = new Random(2);
		static int numValues = 200000;
		static point[] allData = new point[numValues];

		static (double, double)[] klusters = new (double, double)[numKlusters];
		public struct point
		{
			public (double, double) Data;
			public int Kluster;
		}

		static void Main(string[] args)
		{
			var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations);

			var lines = System.IO.File.ReadAllLines("benchmarks/kmeans_concurrent/points.txt");
			bm.Run(() =>
			{
				var initState = generateData(lines);
				allData = initState;
				setKlusters();
				var hasMoved = true;

				while (hasMoved)
				{
					assignPointsToKluster();
					hasMoved = setCenter();
				}
				return true;
			}, (res) =>
			{
				printKlusters();
			});
		}

		public static point[] generateData(string[] lines)
		{
			var initState = new point[numValues];
			var i = 0;
			foreach (var line in lines)
			{
				var split = line.Split(':');
				var num1 = double.Parse(split[0]);
				var num2 = double.Parse(split[1]);
				initState[i] = new point { Kluster = 1, Data = (num1, num2) };
				i++;
			}
			return initState;
		}

		public static void setKlusters()
		{
			for (int i = 0; i < numKlusters; i++)
			{
				klusters[i] = allData[i].Data;
			}
		}

		public static void printKlusters()
		{
			for (int i = 0; i < numKlusters; i++)
			{
				Console.WriteLine($"Kluster {i}: {klusters[i]}");
			}
		}

		public static double distance((double x, double y) a, (double x, double y) b)
		{
			return Math.Sqrt(Math.Pow((b.x - a.x), 2.0) + Math.Pow((b.y - a.y), 2.0));
		}

		public static void assignPointsToKluster()
		{
			Parallel.For(0, allData.Length, i =>
			{
				var nearest = 0;
				var length = Double.PositiveInfinity;
				for (int j = 0; j < numKlusters; j++)
				{
					var tempDist = distance(allData[i].Data, klusters[j]);
					if (tempDist < length)
					{
						length = tempDist;
						nearest = j;
					}
				}
				allData[i].Kluster = nearest;
			});
		}

		public static bool setCenter()
		{
			((double, double), int)[] sums = new ((double, double), int)[numKlusters];

			foreach (var point in allData)
			{
				var (x, y) = point.Data;
				var ((totalX, totalY), num) = sums[point.Kluster];
				totalX = totalX + x;
				totalY = totalY + y;
				num++;
				sums[point.Kluster] = ((totalX, totalY), num);
			}

			var hasMoved = false;
			for (int i = 0; i < numKlusters; i++)
			{
				var ((totalX, totalY), num) = sums[i];
				var (oldX, oldY) = klusters[i];
				var (newX, newY) = (totalX / (double)num, totalY / (double)num);

				if (oldX != newX || oldY != newY)
				{
					hasMoved = true;
				}

				klusters[i] = (newX, newY);
			}
			return hasMoved;
		}
	}
}
