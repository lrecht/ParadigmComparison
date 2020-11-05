using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            Life gameOf = new Life(new GameRules(), 256);
            for (int i = 0; i < 100; i++)
                gameOf.NextGeneration();
            System.Console.WriteLine(gameOf.GetLiveCount());
        }
    }

    public interface IRules
    {
        bool Apply(bool cellValue, int liveNeighbourCount);
    }

    public class GameRules : IRules
    {
        public bool Apply(bool cellValue, int liveNeighbourCount) =>
            // A live cell dies unless it has exactly 2 or 3 live neighbors.
            // A dead cell remains dead unless it has exactly 3 live neighbors.
            cellValue && (liveNeighbourCount == 2 || liveNeighbourCount == 3) || !cellValue && liveNeighbourCount == 3;
    }

    public class Board
    {
        bool[,] _board;
        public int Size { get; }
        public Board(int size)
        {
            Size = size;
        }
        public void Initialise()
        {
            _board = new bool[Size, Size];
            var f = File.ReadAllText("benchmarks/game_of_life_concurrent/state256.txt").Select(c => c == '1').ToArray();
            var len = f.Length;
            for (int i = 0; i < len; i++)
                _board[(i / Size), (i % Size)] = f[i];
        }

        public bool GetCell(int x, int y) => _board[x, y];
        public void Update(bool[,] newBoard) => _board = newBoard;
        public int GetLiveCount() => _board.Cast<bool>().ToList().Where(c => c).Count();

    }

    public class Life
    {
        Board board;
        IRules _gameRules;
        public Life(IRules gameRules, int boardsize)
        {
            board = new Board(boardsize);
            board.Initialise();
            _gameRules = gameRules;
        }

        public int GetLiveCount() => board.GetLiveCount();

        public void NextGeneration()
        {
            int boardSize = board.Size;
            // A temp variable to hold the next state while it's being calculated.
            bool[,] newBoard = new bool[boardSize, boardSize];

            Parallel.For(0, boardSize, y =>
            {
                Parallel.For(0, boardSize, x =>
                {
                    var n = countLiveNeighbors(x, y, boardSize);
                    var c = board.GetCell(x, y);
                    newBoard[x, y] = _gameRules.Apply(c, n);
                });
            });
            // Set the board to its new state.
            board.Update(newBoard);
        }

        // Returns the number of live neighbors around the cell at position (x,y).
        private int countLiveNeighbors(int x, int y, int boardSize)
        {
            // The number of live neighbors.
            int value = 0;

            // This nested loop enumerates the 9 cells in the specified cells neighborhood.
            for (var j = -1; j <= 1; j++)
            {
                // Loop around the edges if y+j is off the board.
                int k = (y + j + boardSize) % boardSize;

                for (var i = -1; i <= 1; i++)
                {
                    // Loop around the edges if x+i is off the board.
                    int h = (x + i + boardSize) % boardSize;

                    // Count the neighbor cell at (h,k) if it is alive.
                    value += board.GetCell(h, k) ? 1 : 0;
                }
            }
            // Subtract 1 if (x,y) is alive since we counted it as a neighbor.
            return value - (board.GetCell(x, y) ? 1 : 0);
        }
    }
}
