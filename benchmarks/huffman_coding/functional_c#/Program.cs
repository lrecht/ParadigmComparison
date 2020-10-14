using System.IO;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace functional_c_
{
    class Program
    {


        static void Main(string[] args)
        {
            var TEST_STRING = File.ReadAllText("benchmarks/huffman_coding/lines.txt");
            var frequencies = getFrequencies(TEST_STRING);
            var mappings = createMappings(frequencies);
            var encodedCharacters = TEST_STRING.Select(x => mappings[x]);
            var encodedString = string.Join("", TEST_STRING.Select(x => mappings[x]));
            System.Console.WriteLine(encodedString.Length);
        }

        private static ImmutableDictionary<char, string> createMappings(IEnumerable<(char, int)> frequencies)
        {
            var tree = frequencies.Select(x => (x.Item2, ImmutableDictionary<char, string>.Empty.Add(x.Item1, "")))
                .ToImmutableSortedSet(Comparer<(int, ImmutableDictionary<char, string>)>
                    .Create((x, y) => x.Item1 > y.Item1 ? 1 : x.Item1 < y.Item1 ? -1 : x.Item2.First().Key.CompareTo(y.Item2.First().Key)));


            return createMappingHelper(tree).First().Item2;
        }

        private static ImmutableSortedSet<(int, ImmutableDictionary<char, string>)> createMappingHelper(ImmutableSortedSet<(int, ImmutableDictionary<char, string>)> tree)
        {
            if(tree.Count() <= 1)
                return tree;

            var elems = tree.Take(2);

            var elem1 = elems.ElementAt(0);
            var elem2 = elems.ElementAt(1);

            var elem1Dictionary = elem1.Item2;
            var elem2Dictionary = elem2.Item2;

            var updatedEncodings1 = elem1Dictionary.Select(x => new KeyValuePair<char, string>(x.Key, "0" + x.Value));
            var updatedEncodings2 = elem2Dictionary.Select(x => new KeyValuePair<char, string>(x.Key, "1" + x.Value));

            var newDict = updatedEncodings1.Union(updatedEncodings2).ToImmutableDictionary();
            var newTup = (elem1.Item1 + elem2.Item1, newDict);

            return createMappingHelper(tree.Except(elems).Add(newTup));
        }

        private static IEnumerable<(char, int)> getFrequencies(string str)
        {
            return str
                .GroupBy(x => x)
                .Select(x => (x.First(), x.Count()));
        }
    }
}
