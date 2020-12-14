using System;

namespace procedural_c_
{
	class Program
	{
		public struct Edge
		{
			public int Start;
			public int End;
			public int Weight;
		}

		static void Main(string[] args)
		{
			var arr = ReadFileToArr();
			System.Console.WriteLine(arr.Length);
		}

		public static Edge[] ReadFileToArr()
		{
			var lines = System.IO.File.ReadAllLines("iotest/spanning_tree/graph.csv");
			var c = lines.Length;
			Edge[] arr = new Edge[c];
			for (int i = 0; i < c;i++)
			{
				var split = lines[i].Split(',');
				arr[i] = new Edge { Start = int.Parse(split[0]), End = int.Parse(split[1]), Weight = int.Parse(split[2]) };
			}
			return arr;
		}
	}
}
