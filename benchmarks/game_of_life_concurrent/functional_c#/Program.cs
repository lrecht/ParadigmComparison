using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using benchmark;

namespace functional_c_
{
	class Program
	{
		static readonly int dimensions = 256;
		static readonly int runs = 100;
		static ImmutableArray<(int x, int y)> relativePostions = Enumerable.Range(-1, 3)
							.SelectMany(x => Enumerable.Range(-1, 3).Select(y => (x, y)))
							.Except(ImmutableList.Create<(int, int)>((0, 0)))
							.ToImmutableArray();
		static void Main(string[] args)
		{
			var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations);

			var initialStateRep = System.IO.File.ReadAllText("benchmarks/game_of_life_concurrent/state256.txt")
				.Select(x => x == '1')
				.ToImmutableArray();

			bm.Run(() =>
			{
				var result = simulateSteps(initialStateRep, runs);
				return result.Where(x => x).Count();
			}, (res) =>
			{
				System.Console.WriteLine(res);
			});
		}

		private static ImmutableArray<bool> simulateSteps(ImmutableArray<bool> state, int runs)
		{
			return Enumerable.Range(1, runs).Aggregate(state, (accState, _) =>
				accState.AsParallel()
					.WithMergeOptions(ParallelMergeOptions.FullyBuffered)
					.Select((x, i) =>
					{
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
