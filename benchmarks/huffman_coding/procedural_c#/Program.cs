using System;
using System.Text;
using System.Collections.Generic;

namespace procedural_c_
{
	class Program
	{
		static void Main(string[] args)
		{
			var text = System.IO.File.ReadAllText("benchmarks/huffman_coding/lines.txt");
			var feq = CreateFrequencies(text);
			var mappings = CreateMappings(feq);
			var encodedString = Encode(mappings, text);
			Console.WriteLine("Length: " + encodedString.Length);
		}

		static Dictionary<char, int> CreateFrequencies(string text)
		{
			var result = new Dictionary<char, int>();
			foreach (var c in text)
			{
				if (result.ContainsKey(c))
				{
					result[c]++;
				}
				else
				{
					result.Add(c, 1);
				}
			}
			return result;
		}

		static Dictionary<char, string> CreateMappings(Dictionary<char, int> frequencies)
		{
			//insert leafs
			foreach (var feq in frequencies)
			{
				insert((feq.Value, new Dictionary<char, string>() { { feq.Key, "" } }));
			}

			while (heap.size > 1)
			{
				var first = pop();
				var second = pop();
				var newElm = (first.Item1 + second.Item1, new Dictionary<char, string>());
				foreach (var elm in first.Item2)
				{
					newElm.Item2.Add(elm.Key, '0' + elm.Value);
				}
				foreach (var elm in second.Item2)
				{
					newElm.Item2.Add(elm.Key, '1' + elm.Value);
				}
				insert(newElm);
			}

			return pop().Item2;
		}

		static string Encode(Dictionary<char, string> mappings, string text)
		{
			var result = new StringBuilder();
			foreach (var c in text)
			{
				result.Append(mappings[c]);
			}
			return result.ToString();
		}


		// --- Heap suff here
		public static Heap heap = new Heap() { array = new (int, Dictionary<char, string>)[1024], maxSize = 1024, size = 0 };
		public static Dictionary<int, int> positions = new Dictionary<int, int>();

		public struct Heap
		{
			public int size;
			public int maxSize;
			public (int, Dictionary<char, string>)[] array;
		}

		public static void replace(int pos, Dictionary<char, string> newDist)
		{
			if (positions.ContainsKey(pos))
			{
				int index = positions[pos];
				swap(index, 0);
				pop();
			}
			positions[pos] = heap.size;
			insert((pos, newDist));
		}

		public static void insert((int, Dictionary<char, string>) element)
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

		public static (int, Dictionary<char, string>) pop()
		{
			(int, Dictionary<char, string>) res = heap.array[0];
			heap.array[0] = heap.array[heap.size - 1];
			heap.size -= 1;
			heapify(0);

			//Maintain dict
			positions.Remove(res.Item1);

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
			// Maintains dictionary to find items later
			positions[heap.array[index1].Item1] = index2;
			positions[heap.array[index2].Item1] = index1;

			var swap = heap.array[index1];
			heap.array[index1] = heap.array[index2];
			heap.array[index2] = swap;
		}

		public static bool smallerThan((int, Dictionary<char, string>) elem1, (int, Dictionary<char, string>) elem2)
		{
			return elem1.Item1 < elem2.Item1;
		}
	}
}