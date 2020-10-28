using System;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;

namespace oop_c_
{
    class Program
    {
        //static string TEST_STRING = File.ReadAllText("benchmarks/playfair_cipher/lines.txt");
        static string TEST_STRING = File.ReadAllText("../lines.txt");
        static void Main(string[] args)
        {
            PlayFairCipher p = new PlayFairCipher("playfair example");
            string encrypt = p.Encrypt(TEST_STRING);
            System.Console.WriteLine(encrypt.Length);
            string decrypt = p.Decrypt(encrypt);
            System.Console.WriteLine(decrypt.Length);
        }
    }

    public class Table
    {
        private char[,] charTable;
        private Point[] positions;
        private Point currentPos;
        public Table()
        {
            charTable = new char[5, 5];
            positions = new Point[26];
            currentPos = new Point();
        }

        public void AddNext(char c)
        {
            charTable[currentPos.X, currentPos.Y] = c;
            positions[c - 'A'] = currentPos;
            if (currentPos.Y == 4 && currentPos.X != 4)
            {
                currentPos.X++;
                currentPos.Y = 0;
            }
            else if (currentPos.X == 5)
                throw new IndexOutOfRangeException();
            else
                currentPos.Y++;
        }

        public bool ContainsChar(char c) => !(positions[c - 'A'] == Point.Empty) || charTable[0,0] == c;
        public Point GetPositionFromChar(char c) => positions[c - 'A'];
        public char GetCharFromPosition(int x, int y) => charTable[x,y];
    }

    public class PlayFairCipher
    {
        private Table charTable { get; set; }
        private Point[] positions { get; set; }
        private string _key { get; }

        public PlayFairCipher(string key)
        {
            _key = key;
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            charTable = new Table();
            createTable(preprocessText(key + alphabet));
            
        }

        public string Encrypt(string text)
        {
            StringBuilder sb = new StringBuilder(preprocessText(text));

            for (int i = 0; i < sb.Length; i += 2)
            {
                // If length is odd add X
                if (i == sb.Length - 1)
                    sb.Append(sb.Length % 2 == 1 ? "X" : "");

                // If two adjacent characters are the same replace last with X
                else if (sb[i] == sb[i+1])
                    sb.Insert(i + 1, 'X');
            }
            return cipher(sb.ToString(), true);
        }

        public string Decrypt(string text) => cipher(text, false);

        private string cipher(string text, bool encipher = true)
        {
            int len = text.Length;
            StringBuilder returnValue = new StringBuilder();
            int e = encipher ? 1 : 4;
            for (int i = 0; i < len; i += 2)
            {
                (char a, char b) = (text[i], text[i+1]);
                (Point aCoords, Point bCoords) = (charTable.GetPositionFromChar(a),charTable.GetPositionFromChar(b));
                if (aCoords.X == bCoords.X)
                    returnValue.Append(sameRow(aCoords, bCoords, e));
                else if(aCoords.Y == bCoords.Y)
                    returnValue.Append(sameColumn(aCoords, bCoords, e));
                else
                    returnValue.Append(differentRowColumn(aCoords, bCoords, e));
            }
            return returnValue.ToString();
        }

        private char[] sameRow(Point aCoords, Point bCoords, int e) => new char[2] {
                charTable.GetCharFromPosition(aCoords.X, (aCoords.Y + e) % 5),
                charTable.GetCharFromPosition(bCoords.X, (bCoords.Y + e) % 5)
        };
        
        private char[] sameColumn(Point aCoords, Point bCoords, int e) => new char[2] {
                charTable.GetCharFromPosition((aCoords.X + e) % 5, aCoords.Y),
                charTable.GetCharFromPosition((bCoords.X + e) % 5, bCoords.Y)
        };

        private char[] differentRowColumn(Point aCoords, Point bCoords, int e) => new char[2] {
                charTable.GetCharFromPosition(aCoords.X,bCoords.Y),
                charTable.GetCharFromPosition(bCoords.X,aCoords.Y)
        };
        

        private string preprocessText(string text) 
        {
            Regex reg = new Regex("[^A-Z']");
            text = text.ToUpper().Replace("J", "I");
            text = reg.Replace(text, string.Empty);
            return text;
        } 

        private void createTable(string key)
        {
            int len = key.Length;
            for (int i = 0; i < len; i++)
            {
                char c = key[i];
                if (!(charTable.ContainsChar(c)))
                    charTable.AddNext(c);
            }
        }
    }
}
