using System;
using System.Linq;
using System.Collections.Immutable;
using benchmark;
using System.Collections.Generic;

namespace functional_c_
{
	class Program
	{
		static readonly string keyword = "this is a great keyword";
		static readonly string alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
		static readonly char rare = 'X';
		static void Main(string[] args)
		{
			var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations);

			var TEST_STRING = System.IO.File.ReadAllText("benchmarks/playfair_cipher/lines.txt");

			bm.Run(() =>
			{
				var table = createTable(keyword);
				var encoded = new string(encode(TEST_STRING, table).ToArray());
				var decoded = new string(decode(encoded, table).ToArray());
				return (encoded.Length, decoded.Length);
			}, (things) =>
			{
				System.Console.WriteLine(things.Item1);
				System.Console.WriteLine(things.Item2);
			});
		}

		private static (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) createTable(string keyword)
			=> createTableHelper(prepKey(keyword), 0, 1, 1, (ImmutableDictionary<char, (int, int)>.Empty, ImmutableDictionary<(int, int), char>.Empty));

		private static (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) createTableHelper(
			ImmutableArray<char> keyword,
			int keywordIndex,
			int rowIndex,
			int colIndex,
			(ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
		{
			if (keywordIndex >= keyword.Length)
				return table;

			var (postions, values) = table;
			var c = keyword[keywordIndex];
			var updatedPoss = postions.Add(c, (rowIndex, colIndex));
			var updatedVals = values.Add((rowIndex, colIndex), c);
			return createTableHelper(keyword,
									keywordIndex + 1,
									((rowIndex % 5) + 1),
									(colIndex + rowIndex / 5),
									(updatedPoss, updatedVals));
		}

		private static ImmutableArray<char> prepKey(string keyword)
		{
			var simpleKey = keyword
							.Where(x => char.IsLetter(x))
							.Distinct()
							.Select(x => char.ToUpper(x == 'j' || x == 'J' ? 'I' : x))
							.ToImmutableArray();

			var smallerAlph = alphabet
							.Where(x => !simpleKey.Contains(x))
							.ToImmutableArray();

			return simpleKey.AddRange(smallerAlph);
		}

		private static ImmutableArray<char> prepInput(string str)
			=> str
				.Select(c => char.ToUpper(c == 'j' || c == 'J' ? 'I' : c))
				.Where(c => alphabet.Contains(c))
				.ToImmutableArray();

		private static string encode(string str, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
			=> cipher(encodePair, str, table);

		private static string decode(string str, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
			=> cipher(decodePair, str, table);

		private static string cipher(
			Func<char, char, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>), (char c1, char c2)> func,
			string str,
			(ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
		{
			var prep = prepInput(str);

			var insertedRare = prep.Aggregate(ImmutableList<char>.Empty, ((acc, c) => acc.Count % 2 == 1 && acc.Last() == c ? acc.Add(rare).Add(c) : acc.Add(c)));
			var evenLengthString = (insertedRare.Count % 2 == 0 ? insertedRare : insertedRare.Add(rare)).ToImmutableArray();

			var charPairs = Enumerable.Range(0, evenLengthString.Length / 2)
				.Select(i => func(evenLengthString[i * 2], evenLengthString[i * 2 + 1], table));

			return string.Join("", charPairs.Select(t => t.c1.ToString() + t.c2.ToString()));
		}

		private static (char, char) encodePair(
			char first,
			char second,
			(ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
				=> pairHelper((x => x % 5 + 1), first, second, table);

		private static (char, char) decodePair(
			char first,
			char second,
			(ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
				=> pairHelper((x => x == 1 ? 5 : x - 1), first, second, table);

		private static (char, char) pairHelper(
			Func<int, int> adjust,
			char first,
			char second,
			(ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
		{
			var (col1, row1) = findPos(first, table);
			var (col2, row2) = findPos(second, table);

			if (first == second)
				return pairHelper(adjust, first, rare, table);
			else if (row1 == row2)
			{
				var c1 = findVal(adjust(col1), row1, table);
				var c2 = findVal(adjust(col2), row2, table);
				return (c1, c2);
			}
			else if (col1 == col2)
			{
				var c1 = findVal(col1, adjust(row1), table);
				var c2 = findVal(col2, adjust(row2), table);
				return (c1, c2);
			}
			else
			{
				var c1 = findVal(col2, row1, table);
				var c2 = findVal(col1, row2, table);
				return (c1, c2);
			}
		}

		private static (int, int) findPos(char c, (ImmutableDictionary<char, (int, int)> positions, ImmutableDictionary<(int, int), char>) table)
			=> table.positions[c];

		private static char findVal(int x, int y, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char> values) table)
			=> table.values[(x, y)];

	}
}
