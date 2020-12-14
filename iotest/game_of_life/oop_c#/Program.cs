using System.IO;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var f = File.ReadAllText("iotest/game_of_life/state256.txt").Select(c => c == '1').ToArray();
            System.Console.WriteLine(f.Length);
        }
    }
}
