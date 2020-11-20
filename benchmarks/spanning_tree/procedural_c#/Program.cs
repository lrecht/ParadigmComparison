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
			//Init the vertex groups
			vertexGroups = new int[6005 + 1];
			for (int i = 0; i < vertexGroups.Length; i++)
			{
				vertexGroups[i] = -1;
			}

			//Do the spanning
			var arr = ReadFileToArr();
			Quick_Sort(arr,0,arr.Length-1);
			var (weight, edges) = computeMinspanTree(arr);

			Console.WriteLine("Total weight: " + weight);
			Console.WriteLine("Total Edges: " + edges);
		}

		public static Edge[] ReadFileToArr()
		{
			var lines = System.IO.File.ReadAllLines("/home/lars/Documents/9. Semester/ParadigmComparison/benchmarks/spanning_tree/graph.csv");//("benchmarks/spanning_tree/graph.csv");
			var c = lines.Length;
			Edge[] arr = new Edge[c];
			for (int i = 0; i < c;i++)
			{
				var split = lines[i].Split(',');
				arr[i] = new Edge { Start = int.Parse(split[0]), End = int.Parse(split[1]), Weight = int.Parse(split[2]) };
			}
			return arr;
		}

		public static (int, int) computeMinspanTree(Edge[] arr)
		{
			var magic = 5877 - 1;
			var result = new Edge[magic];
			var size = 0;
			var totalWeight = 0;
			var totalEdges = 0;
			var i = 0;
			while (size < magic)
			{
				var current = arr[i++];
				if (union(current.Start, current.End))
				{
					result[size] = current;
					size++;
					totalWeight += current.Weight;
					totalEdges++;
				}
			}
			return (totalWeight, totalEdges);
		}

		public static int[] vertexGroups;
		public static int unionFind(int node)
		{
			if (vertexGroups[node] < 0)
			{
				return node;
			}
			else
			{
				vertexGroups[node] = unionFind(vertexGroups[node]);
				return vertexGroups[node];
			}
		}

		public static bool union(int startNode, int endNode)
		{
			var group1Root = unionFind(startNode);
			var group2Root = unionFind(endNode);
			if (group1Root == group2Root)
			{
				return false;
			}
			else
			{
				vertexGroups[group2Root] = group1Root;
				return true;
			}
		}

		// Made by https://www.w3resource.com/csharp-exercises/searching-and-sorting-algorithm/searching-and-sorting-algorithm-exercise-9.php
		private static void Quick_Sort(Edge[] arr, int left, int right) 
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);

                if (pivot > 1)
                    Quick_Sort(arr, left, pivot - 1);

                if (pivot + 1 < right)
                    Quick_Sort(arr, pivot + 1, right);
            }
        }

        private static int Partition(Edge[] arr, int left, int right)
        {
            int pivot = arr[left].Weight;
            while (true) 
            {
                while (arr[left].Weight < pivot) 
                    left++;

                while (arr[right].Weight > pivot)
                    right--;

                if (left < right)
                {
                    if (arr[left].Weight == arr[right].Weight){
						left++;
						right--;
					}
					else {
						Edge temp = arr[left];
						arr[left] = arr[right];
						arr[right] = temp;
					}
                }
                else 
                    return right;
            }
        }
	}
}
