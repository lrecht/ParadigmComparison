using System;
using System.Threading;
using System.Threading.Tasks;
using benchmark;

namespace procedural_c_
{
	class Program
	{
		static int runs = 100;
		static int height = 256;
		static int width = 256;
		static bool[,] board = new bool[width, height];

		static void Main(string[] args)
		{
			var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations);
			var file = System.IO.File.ReadAllText("benchmarks/game_of_life_concurrent/state256.txt");

			bm.Run(() =>
			{
				var initState = initilizeBoard(file);
				board = initState;
				for (int i = 0; i < runs; i++)
				{
					updateBoard();
				}
				return countAlive();
			}, (res) =>
			{
				Console.WriteLine("Alive: " + res);
			});
		}

		public static int countLiveNeighbors(int x, int y)
		{
			int value = 0;
			for (int j = -1; j <= 1; j++)
			{
				int k = ((((y + j + height) % height) + height) % height);
				for (int i = -1; i <= 1; i++)
				{
					int h = ((((x + i + width) % width) + width) % width);
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


		public static void updateBoard()
		{
			var newBoard = new bool[width, height];

			Parallel.For(0, width, x =>
			{
				Parallel.For(0, height, y =>
				{
					var n = countLiveNeighbors(x, y);
					var c = board[x, y];
					newBoard[x, y] = c && (n == 2 || n == 3) || (!c && n == 3);
				});
			});

			board = newBoard;
		}

		public static bool[,] initilizeBoard(string state)
		{
			var initState = new bool[height, width];
			for (int i = 0; i < state.Length; i++)
			{
				initState[(i / width), (i % width)] = state[i] == '1';
			}
			return initState;
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
