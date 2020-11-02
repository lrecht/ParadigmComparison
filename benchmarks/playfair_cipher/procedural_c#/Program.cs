using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace procedural_c_
{
	class Program
	{
		const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const int dimension1 = 5;
		const int dimension2 = 5;
		static char[,] table;
		static (int, int)[] positions;

		static void Main(string[] args)
		{
			positions = new (int, int)[26];

			table = new char[dimension1, dimension2];
			var text = System.IO.File.ReadAllText("benchmarks/playfair_cipher/lines.txt");
			var keyword = "This is a great keyword";

			populateTable(preprocessText(keyword + alphabet));
			text = preprocessText(text);
			string encryption = encrypt(text);
			string decryption = decrypt(encryption);

			Console.WriteLine(encryption.Length);
			Console.WriteLine(decryption.Length);
		}

		static string preprocessText(string text)
		{
			text = text.ToUpper().Replace("J", "I");
			return Regex.Replace(text, "[^A-Z]", "");
		}

		static void populateTable(string text)
		{
			var textLength = text.Length;
			var place = 0;
			for (int i = 0; i < textLength; i++)
			{
				var pos = (place / dimension1, place % dimension1);
				var character = text[i];
				var charPos = (character - 'A');
				if (positions[charPos] == (0, 0) && table[0, 0] != character)
				{
					table[pos.Item1, pos.Item2] = character;
					positions[charPos] = pos;
					place++;
				}
			}
		}

		static string encrypt(string text)
		{
			StringBuilder sb = new StringBuilder(text);
			for (int i = 0; i < sb.Length; i += 2)
			{
				if (i == sb.Length - 1)
				{
					sb = sb.Append(sb.Length % 2 == 1 ? "X" : "");
				}

				else if (sb[i] == sb[i + 1])
					sb = sb.Insert(i + 1, 'X');
			}

			return iterateOnPairs(sb.ToString(), 1);
		}

		static string decrypt(string text)
		{
			return iterateOnPairs(text, 4);
		}

		static string iterateOnPairs(string text, int direction)
		{
			var result = new StringBuilder();
			int i = 0;
			while (i < text.Length)
			{
				var first = positions[(text[i] - 'A')];
				var second = positions[(text[i + 1] - 'A')];
				result = result.Append(cipher(first, second, direction));
				i += 2;
			}
			return result.ToString();
		}

		static string cipher((int, int) item1, (int, int) item2, int direction)
		{
			if (item1.Item2 == item2.Item2)
			{
				item1.Item1 = (item1.Item1 + direction) % 5;
				item2.Item1 = (item2.Item1 + direction) % 5;
			}
			else if (item1.Item1 == item2.Item1)
			{
				item1.Item2 = (item1.Item2 + direction) % 5;
				item2.Item2 = (item2.Item2 + direction) % 5;
			}
			else
			{
				int tmp = item1.Item1;
				item1.Item1 = item2.Item1;
				item2.Item1 = tmp;
			}

			char value1 = table[item1.Item1, item1.Item2];
			char value2 = table[item2.Item1, item2.Item2];
			return value1.ToString() + value2.ToString();
		}
	}
}
