using System.Linq;
using System.Collections.Immutable;

namespace functional_c_
{
    class Program
    {
        static readonly int dimensions = 256;
        static readonly int runs = 100;
        static ImmutableArray<(int x, int y)> relativePostions = Enumerable.Range(-1, 3)
                            .SelectMany(x => Enumerable.Range(-1, 3).Select(y => (x, y)))
                            .Except(ImmutableList.Create<(int, int)>((0,0)))
                            .ToImmutableArray();
        static void Main(string[] args)
        {
            var initialStateRep = System.IO.File.ReadAllText("benchmarks/game_of_life/state256.txt")
                .Select(x => x == '1')
                .ToImmutableArray();
            
            var result = simulateSteps(initialStateRep, runs);
            
            System.Console.WriteLine(result.Count(x => x));
        }

        private static ImmutableArray<bool> simulateSteps(ImmutableArray<bool> state, int runs)
        {
            return Enumerable.Range(1, runs).Aggregate(state, (accState, _) =>
                accState.Select((x, i) => {
                    var neighbourCoordinates = relativePostions
                                            .Select(pos => (
                                                ((((i % dimensions + pos.x) % dimensions) + dimensions) % dimensions), 
                                                ((((i / dimensions + pos.y) % dimensions) + dimensions) % dimensions)));

                    var neighbourIndecies = neighbourCoordinates.Select(x => (dimensions * x.Item2 + x.Item1));

                    var liveNeighbours = neighbourIndecies.Where(j => accState[j]).Count();

                    return x ? liveNeighbours == 2 || liveNeighbours == 3 : liveNeighbours == 3;
                }).ToImmutableArray()
            );
        }
    }
}
