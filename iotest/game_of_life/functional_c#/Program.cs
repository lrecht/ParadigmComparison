using System.Linq;
using System.Collections.Immutable;

namespace functional_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var initialStateRep = System.IO.File.ReadAllText("iotest/game_of_life/state256.txt")
                .Select(x => x == '1')
                .ToImmutableArray();

            System.Console.WriteLine(initialStateRep.Count());
        }
    }
}
