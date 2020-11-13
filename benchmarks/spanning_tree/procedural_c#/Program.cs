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
			ReadFileToHeap();
			var (weight, edges) = computeMinspanTree();

			Console.WriteLine("Total weight: " + weight);
			Console.WriteLine("Total Edges: " + edges);
		}

		public static void ReadFileToHeap()
		{
			var lines = System.IO.File.ReadAllLines("benchmarks/spanning_tree/graph.csv");
			var i = 0;
			foreach (var line in lines)
			{
				var split = line.Split(',');
				insert(new Edge { Start = int.Parse(split[0]), End = int.Parse(split[1]), Weight = int.Parse(split[2]) });
			}
		}

		public static (int, int) computeMinspanTree()
		{
			var magic = 5877 - 1;
			var result = new Edge[magic];
			var size = 0;
			var totalWeight = 0;
			var totalEdges = 0;
			while (size < magic)
			{
				var current = pop();
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

		// --- Heap suff here
		public static Heap heap = new Heap()
		{
			array = new Edge[1024],
			maxSize = 1024,
			size = 0
		};

		public struct Heap
		{
			public int size;
			public int maxSize;
			public Edge[] array;
		}

		public static void insert(Edge element)
		{
			if (heap.size == heap.maxSize)
			{
				Array.Resize(ref heap.array, heap.size * 2);
				heap.maxSize *= 2;
			}

			heap.array[heap.size] = element;
			heap.size += 1;
			heapifyNode(heap.size - 1);
		}

		public static Edge pop()
		{
			Edge res = heap.array[0];
			heap.array[0] = heap.array[heap.size - 1];
			heap.size -= 1;
			heapify(0);

			return res;
		}

		public static void heapifyNode(int index)
		{
			// Find parent 
			int parent = (index - 1) / 2;

			// For Max-Heap 
			// If current node is greater than its parent 
			// Swap both of them and call heapify again 
			// for the parent 
			if (smallerThan(heap.array[index], heap.array[parent]))
			{
				swap(index, parent);

				// Recursively heapify the parent node 
				heapifyNode(parent);
			}
		}

		public static void heapify(int index)
		{
			// Code from https://www.geeksforgeeks.org/heap-sort/
			int smallest = index; // Initialize smallest as root 
			int l = 2 * index + 1; // left = 2*i + 1 
			int r = 2 * index + 2; // right = 2*i + 2 

			// If left child is smaller than root 
			if (l < heap.size && smallerThan(heap.array[l], heap.array[smallest]))
				smallest = l;

			// If right child is smaller than smallest so far 
			if (r < heap.size && smallerThan(heap.array[r], heap.array[smallest]))
				smallest = r;

			// If smallest is not root 
			if (smallest != index)
			{
				swap(index, smallest);

				// Recursively heapify the affected sub-tree 
				heapify(smallest);
			}
		}

		public static void swap(int index1, int index2)
		{
			var swap = heap.array[index1];
			heap.array[index1] = heap.array[index2];
			heap.array[index2] = swap;
		}

		public static bool smallerThan(Edge elem1, Edge elem2)
		{
			return elem1.Weight < elem2.Weight;
		}
		// -- End of heap stuff
	}
}
