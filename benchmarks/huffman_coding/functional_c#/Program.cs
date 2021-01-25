using System.IO;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using benchmark;

namespace functional_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
            var bm = new Benchmark(iterations);
			
			var TEST_STRING = File.ReadAllText("benchmarks/huffman_coding/lines.txt");
            
			bm.Run(() => {
				var frequencies = getFrequencies(TEST_STRING);
				var mappings = createMappings(frequencies);
				var encodedCharacters = TEST_STRING.Select(x => mappings[x]);
				var encodedString = string.Join("", TEST_STRING.Select(x => mappings[x]));
				return encodedString.Length;
			}, (res) => {
				System.Console.WriteLine(res);
			});
        }

        private static ImmutableDictionary<char, string> createMappings(ImmutableArray<(char, int)> frequencies)
        {
            var tree = frequencies.Select(x => (x.Item2, ImmutableArray<(char, string)>.Empty.Add((x.Item1, ""))))
                .ToImmutableSortedSet(Comparer<(int, ImmutableArray<(char, string)>)>
                    .Create((x, y) => x.Item1 > y.Item1 ? 1 : x.Item1 < y.Item1 ? -1 : x.Item2.First().Item1.CompareTo(y.Item2.First().Item1)));

            return createMappingHelper(tree).First().Item2.ToImmutableDictionary(x => x.Item1, elementSelector: y => y.Item2);
        }

        private static ImmutableSortedSet<(int, ImmutableArray<(char, string)>)> createMappingHelper(ImmutableSortedSet<(int, ImmutableArray<(char, string)>)> tree)
        {
            if(tree.Count <= 1)
                return tree;

            var elems = tree.Take(2);

            var elem1 = elems.ElementAt(0);
            var elem2 = elems.ElementAt(1);

            var elem1List = elem1.Item2;
            var elem2List = elem2.Item2;

            var updatedEncodings1 = elem1List.Select(x => (x.Item1, "0" + x.Item2));
            var updatedEncodings2 = elem2List.Select(x => (x.Item1, "1" + x.Item2));

            var newList = updatedEncodings1.Union(updatedEncodings2).ToImmutableArray();
            var newTup = (elem1.Item1 + elem2.Item1, newList);

            return createMappingHelper(tree.Except(elems).Add(newTup));
        }

        private static ImmutableArray<(char, int)> getFrequencies(string str)
        {
            return str
                .GroupBy(x => x)
                .Select(x => (x.First(), x.Count()))
                .ToImmutableArray();
        }
    }
}
