using System;
using System.Threading;

namespace procedural_c_
{
	class Program
	{
		static int runs = 100;
		static int height = 256;
		static int width = 256;
		static int logicalProcessors = Environment.ProcessorCount;
		static bool[,] board = new bool[width, height];

		static void Main(string[] args)
		{
			initilizeBoard();
			for (int i = 0; i < runs; i++)
			{
				updateBoard();
			}
			var count = countAlive();
			Console.WriteLine("Alive: " + count);
		}

		public static int countLiveNeighbors(int x, int y)
		{
			int value = 0;
			for (int j = -1; j <= 1; j++)
			{
				int k = (y + j + height) % height;
				for (int i = -1; i <= 1; i++)
				{
					int h = (x + i + width) % width;
					if (board[h, k])
					{
						value++;
					}
				}
			}

			if (board[x, y])
				value--;
			return value;
		}

		public static void updateBordPartly(int start, int stop, bool[,] newBoard)
		{
			for (int i = start; i <= stop; i++)
			{
				var x = (i / width);
				var y = (i % width);
				var n = countLiveNeighbors(x, y);
				var c = board[x, y];
				newBoard[x, y] = c && (n == 2 || n == 3) || (!c && n == 3);
			}
		}

		public static void updateBoard()
		{
			var newBoard = new bool[width, height];

			Thread[] threadPool = new Thread[logicalProcessors];

			for (int i = 0; i < logicalProcessors; i++)
			{
				var start = i * (width * height) / logicalProcessors;
				var stop = ((i + 1) * (width * height) / logicalProcessors) - 1;
				if (i == logicalProcessors)
				{
					stop--;
				}

				var thread1 = new Thread(() => updateBordPartly(start, stop, newBoard));
				threadPool[i] = thread1;
				thread1.Start();
			}

			for (int i = 0; i < logicalProcessors; i++)
			{
				threadPool[i].Join();
			}

			board = newBoard;
		}

		public static void initilizeBoard()
		{
			var state = System.IO.File.ReadAllText("../state256.txt");
			for (int i = 0; i < state.Length; i++)
			{
				board[(i / width), (i % width)] = state[i] == '1';
			}
		}

		public static int countAlive()
		{
			var count = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (board[x, y])
					{
						count++;
					}
				}
			}
			return count;
		}
	}
}
