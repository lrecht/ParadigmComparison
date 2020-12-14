using System;

namespace procedural_c_
{
	class Program
	{
		static int numValues = 200000;
		static point[] allData = new point[numValues];
		public struct point
		{
			public (double, double) Data;
			public int Cluster;
		}

		static void Main(string[] args)
		{
			generateData();
			System.Console.WriteLine(allData.Length);
		}

		public static void generateData()
		{
			var lines = System.IO.File.ReadAllLines("iotest/kmeans/points.txt");
			var i = 0;
			foreach (var line in lines)
			{
				var split = line.Split(':');
				var num1 = double.Parse(split[0]);
				var num2 = double.Parse(split[1]);
				allData[i] = new point { Cluster = 1, Data = (num1, num2) };
				i++;
			}
		}
	}
}
