﻿using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace functional_c_
{
    class Program
    {
        static readonly int dimensions = 256;
        static readonly int runs = 20;
        static void Main(string[] args)
        {
            var initialStateRep = System.IO.File.ReadAllText("benchmarks/game_of_life/state256.txt");
            var lives = initialStateRep.Select(x => x == '1');
            var cords = Enumerable.Range(0, dimensions);
            var positions = cords.SelectMany(x => cords.Select(y => (x, y)));
            var initialState = positions.Zip(lives, (k, v) => new KeyValuePair<(int x, int y), bool>(k, v)).ToImmutableDictionary<(int x, int y), bool>();
            var result = simulateSteps(initialState, runs);
            var living = result.Where(x => x.Value);

            System.Console.WriteLine(living.Count());
        }

        private static ImmutableDictionary<(int x, int y), bool> simulateSteps(ImmutableDictionary<(int x, int y), bool> state, int runs)
            => runs <= 0 ? state : simulateSteps(getNextState(state), runs - 1);

        private static ImmutableDictionary<(int x, int y), bool> getNextState(ImmutableDictionary<(int x, int y), bool> state)
        {
            var relative = ImmutableList<int>.Empty.Add(-1).Add(0).Add(1);
            return state.Select(point => {
                var neighbourPositions = relative.SelectMany(x => relative.Select(y => (x, y)))
                                        .Select(pos => (((((point.Key.x + pos.x) % dimensions) + dimensions) % dimensions) , ((((point.Key.y + pos.y) % dimensions) + dimensions) % dimensions)))
                                        .Where(pos => point.Key != pos);

                var liveNeighboursCount = neighbourPositions.Where(pos => state[pos]).Count();

                if(point.Value){
                    return new KeyValuePair<(int x, int y), bool>(point.Key, liveNeighboursCount == 2 || liveNeighboursCount == 3);
                }
                else{
                    return new KeyValuePair<(int x, int y), bool>(point.Key, liveNeighboursCount == 3);
                }
            })
            .ToImmutableDictionary();
        }
    }
}
