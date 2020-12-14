using System;

namespace procedural_c_
{
	class Program
	{
		static int width = 256;
		static bool[,] board = new bool[width, width];

		static void Main(string[] args)
		{
			initilizeBoard();
			Console.WriteLine(board.Length);
		}


		public static void initilizeBoard()
		{
			var state = System.IO.File.ReadAllText("iotest/game_of_life/state256.txt");
			for (int i = 0; i < state.Length; i++)
			{
				board[(i / width), (i % width)] = state[i] == '1';
			}
		}
	}
}
