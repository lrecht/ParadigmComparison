﻿using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace functional_c_
{
    class Program
    {
        static readonly int dimensions = 256;
        static readonly int size = dimensions * dimensions;
        static readonly int runs = 100;
        static readonly int threads = System.Environment.ProcessorCount;
        static void Main(string[] args)
        {
            var initialStateRep = System.IO.File.ReadAllText("benchmarks/game_of_life/state256.txt")
                .Select(x => x == '1')
                .ToImmutableArray();
            
            var result = simulateSteps(initialStateRep, runs);
            
            var living = result.Where(x => x);
            System.Console.WriteLine(living.Count());
        }

        private static ImmutableArray<bool> simulateSteps(ImmutableArray<bool> state, int runs)
            => runs <= 0 ? state : simulateSteps(getNextState(state), runs - 1);

        private static ImmutableArray<bool> getNextState(ImmutableArray<bool> state)
        {
            var relative = Enumerable.Range(-1, 3).ToImmutableList();
            var relativePositions = relative.SelectMany(x => relative.Select(y => (x, y)));
            return state.AsParallel()
                .WithDegreeOfParallelism(threads)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                .Select((x, i) => {
                    var iCoordinates = ((i % dimensions), (i / dimensions));

                    var neighbourCoordinates = relativePositions
                                        .Select(pos => (
                                            ((((i % dimensions + pos.x) % dimensions) + dimensions) % dimensions), 
                                            ((((i / dimensions + pos.y) % dimensions) + dimensions) % dimensions)))
                                        .Where(x => x != iCoordinates);

                var neighbourIndecies = neighbourCoordinates.Select(x => (dimensions * x.Item2 + x.Item1));

                var liveNeighbours = neighbourIndecies.Where(j => state[j]).Count();

                return x ? liveNeighbours == 2 || liveNeighbours == 3 : liveNeighbours == 3;
            })
            .ToImmutableArray();
        }
    }
}
