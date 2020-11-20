﻿using System;
using System.Linq;
using System.Collections.Immutable;

namespace functional_c_
{
    class Program
    {
        static readonly string keyword = "this is a great keyword";
        static readonly string alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        static readonly char rare = 'X';
        static void Main(string[] args)
        {

            var TEST_STRING = System.IO.File.ReadAllText("benchmarks/playfair_cipher/lines.txt");
            var table = createTable(keyword);
            var encoded = new string(encode(TEST_STRING, table).ToArray());
            var decoded = new string(decode(encoded, table).ToArray());

            System.Console.WriteLine(encoded.Length);
            System.Console.WriteLine(decoded.Length);
        }

        private static (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) createTable(string keyword)
            => createTableHelper(prepKey(keyword), 1, 1, (ImmutableDictionary<char, (int, int)>.Empty, ImmutableDictionary<(int, int), char>.Empty));

        private static (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) createTableHelper(
            ImmutableList<char> keyword, 
            int rowIndex, 
            int colIndex, 
            (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
        {
            if(keyword.IsEmpty)
                return table;
            
            var (postions, values) = table;
            var c = keyword.First();
            var updatedPoss = postions.Add(c, (rowIndex, colIndex));
            var updatedVals = values.Add((rowIndex, colIndex), c);
            return createTableHelper(keyword.RemoveAt(0), 
                                    ((rowIndex % 5) + 1),
                                    (colIndex + rowIndex / 5),
                                    (updatedPoss, updatedVals));
        }

        private static ImmutableList<char> prepKey(string keyword)
        {
            var simpleKey = keyword
                            .Where(x => char.IsLetter(x))
                            .Distinct()
                            .Select(x => char.ToUpper( x == 'j' || x == 'J' ? 'I' : x ))
                            .ToImmutableList();

            var smallerAlph = alphabet
                            .Where(x => !simpleKey.Contains(x))
                            .ToImmutableList();

            return simpleKey.AddRange(smallerAlph);
        }

        private static ImmutableList<char> prepInput(ImmutableList<char> str)
            => str
                .Select(c => char.ToUpper( c == 'j' || c == 'J' ? 'I' : c ))
                .Where(c => alphabet.Contains(c))
                .ToImmutableList();

        private static ImmutableList<char> encode(string str, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
            => cipher(encodePair, str, table);

        private static ImmutableList<char> decode(string str, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
            => cipher(decodePair, str, table);

        private static ImmutableList<char> cipher(
            Func<char, char, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>), ImmutableList<char>> func, 
            string str, 
            (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
        {
            var prep = prepInput(str.ToImmutableList());

            return codeHelper(func, prep, table);
        }

        private static ImmutableList<char> encodePair(
            char first, 
            char second, 
            (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
                => pairHelper((x => x % 5 + 1), first, second, table);

        private static ImmutableList<char> decodePair(
            char first, 
            char second, 
            (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
                => pairHelper((x => x == 1 ? 5 : x - 1), first, second, table);

        private static ImmutableList<char> pairHelper(
            Func<int, int> adjust, 
            char first, 
            char second, 
            (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
        {
            var (col1, row1) = findPos(first, table);
            var (col2, row2) = findPos(second, table);

            if(first == second)
                return pairHelper(adjust, first, rare, table);
            else if(row1 == row2){
                var c1 = findVal(adjust(col1), row1, table);
                var c2 = findVal(adjust(col2), row2, table);
                return ImmutableList<char>.Empty.Add(c1).Add(c2);
            }
            else if(col1 == col2){
                var c1 = findVal(col1, adjust(row1), table);
                var c2 = findVal(col2, adjust(row2), table);
                return ImmutableList<char>.Empty.Add(c1).Add(c2);
            }
            else{
                var c1 = findVal(col2, row1, table);
                var c2 = findVal(col1, row2, table);
                return ImmutableList<char>.Empty.Add(c1).Add(c2);
            }
        }

        private static (int, int) findPos(char c, (ImmutableDictionary<char, (int, int)> positions, ImmutableDictionary<(int, int), char>) table)
            => table.positions[c];

        private static char findVal(int x, int y, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char> values) table)
            => table.values[(x, y)];

        private static ImmutableList<char> codeHelper(
            Func<char, char, (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>), ImmutableList<char>> codeFunc, 
            ImmutableList<char> input,
            (ImmutableDictionary<char, (int, int)>, ImmutableDictionary<(int, int), char>) table)
        {
            var insertedRare = input.Aggregate(ImmutableList<char>.Empty, ((acc, c) => acc.Count % 2 == 1 && acc.Last() == c ? acc.Add(rare).Add(c) : acc.Add(c)));
            var evenLengthString = insertedRare.Count % 2 == 0 ? insertedRare : insertedRare.Add(rare);

            return evenLengthString
                .Where((c, i) => i % 2 == 0)
                .Select((c, i) => (c, evenLengthString[i * 2 + 1]))
                .Select(charPair => codeFunc(charPair.c, charPair.Item2, table))
                .Aggregate((str1, str2) => str1.AddRange(str2));
        }
    }
}
