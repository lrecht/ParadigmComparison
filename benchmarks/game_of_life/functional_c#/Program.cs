using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace functional_c_
{
    class Program
    {
        static readonly int dimensions = 256;
        static readonly int runs = 100;
        static void Main(string[] args)
        {
            var initialStateRep = System.IO.File.ReadAllText("benchmarks/game_of_life/state256.txt")
                .Select(x => x == '1');
            
            var indecies = Enumerable.Range(0, dimensions);
            var positions = indecies.SelectMany(x => indecies.Select(y => (x, y)));
            var initialState = positions
                                .Zip(initialStateRep, (k, v) => new KeyValuePair<(int, int), bool>(k, v))
                                .ToImmutableDictionary<(int x, int y), bool>();
            
            var result = simulateSteps(initialState, runs);
            
            var living = result.Where(x => x.Value);
            System.Console.WriteLine(living.Count());
        }

        private static ImmutableDictionary<(int, int), bool> simulateSteps(ImmutableDictionary<(int, int), bool> state, int runs)
            => runs <= 0 ? state : simulateSteps(getNextState(state), runs - 1);

        private static ImmutableDictionary<(int, int), bool> getNextState(ImmutableDictionary<(int x, int y), bool> state)
        {
            var relative = Enumerable.Range(-1, 3).ToImmutableList();
            return state.Select(point => {
                var neighbourPositions = relative.SelectMany(x => relative.Select(y => (x, y)))
                                        .Select(pos => (
                                            ((((point.Key.x + pos.x) % dimensions) + dimensions) % dimensions), 
                                            ((((point.Key.y + pos.y) % dimensions) + dimensions) % dimensions)))
                                        .Where(pos => point.Key != pos);

                var liveNeighbours = neighbourPositions.Where(pos => state[pos]).Count();

                var newPointValue = point.Value ? liveNeighbours == 2 || liveNeighbours == 3 : liveNeighbours == 3;
                return new KeyValuePair<(int, int), bool>(point.Key, newPointValue);
            })
            .ToImmutableDictionary();
        }
    }
}
