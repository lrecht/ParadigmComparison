using System.Text;
using System.Collections.Generic;
using System;
using System.IO;

namespace oop_c_
{
    class Program
    {
        static string TEST_STRING = File.ReadAllText("../text.txt");

        static void Main(string[] args)
        {
            Huffman huffman = new Huffman(TEST_STRING);
            string encodedString = huffman.Encode(TEST_STRING);
            System.Console.WriteLine(encodedString.Length);
        }
    }

    public abstract class HuffmanTree : IComparable<HuffmanTree>
    {
        public int Id { get; set; } // Nessecary to ensure total order
        public int Frequency { get; }
        public HuffmanTree(int freq, int id) =>
            (Frequency, Id) = (freq, id);

        // Sorts based on lowest frequency
        public int CompareTo(HuffmanTree obj)
        {
            int res = Frequency - obj.Frequency;
            return res == 0 ? Id.CompareTo(obj.Id) : res;
        }
    }

    public class HuffmanLeaf : HuffmanTree
    {
        public char Character { get; } // the character this leaf represents
        public HuffmanLeaf(char c, int freq, int id) : base(freq, id) =>
            Character = c;
    }

    public class HuffmanNode : HuffmanTree
    {
        public HuffmanTree Left, Right; //subtrees
        public HuffmanNode(HuffmanTree left, HuffmanTree right, int id) : 
            base(left.Frequency + right.Frequency, id) =>
                (Left,Right) = (left,right);
    }

    public class Huffman
    {
        Dictionary<char, string> SymbolTable { get; set; }
        public Huffman(string stringToEncode)
        {
            SymbolTable = new Dictionary<char, string>();
            var frequencies = new Dictionary<char, int>();
            // read each character and record the frequencies
            foreach (char c in stringToEncode)
            {
                if (!frequencies.ContainsKey(c))
                    frequencies[c] = 0;
                frequencies[c]++;
            }

            // build tree
            HuffmanTree tree = BuildTree(frequencies);
            UpdateSymbolTable(tree, new StringBuilder());
        }
        // input is an dictionary of frequencies, indexed by character code
        public HuffmanTree BuildTree(Dictionary<char, int> frequencies)
        {
            SortedSet<HuffmanTree> trees = new SortedSet<HuffmanTree>();
            int id = 0;
            // initially, we have a forest of leaves. One for each non-empty character
            foreach (var symbol in frequencies)
                trees.Add(new HuffmanLeaf(symbol.Key, symbol.Value, id++));

            // loop until there is only one tree left
            while (trees.Count > 1)
            {
                // two trees with lowest frequency
                HuffmanTree leftChild = trees.Min;
                trees.Remove(trees.Min);
                HuffmanTree rightChild = trees.Min;
                trees.Remove(trees.Min);

                // put into new node and re-insert into queue
                trees.Add(new HuffmanNode(leftChild, rightChild, id++));
            }
            HuffmanTree root = trees.Min;
            return root;
        }
        public void UpdateSymbolTable(HuffmanTree tree, StringBuilder prefix)
        {
            if (tree is HuffmanLeaf)
            {
                HuffmanLeaf leaf = (HuffmanLeaf)tree;
                SymbolTable[leaf.Character] =  prefix.ToString();
            }
            else if (tree is HuffmanNode)
            {
                HuffmanNode node = (HuffmanNode)tree;
                // traverse left
                prefix.Append("0");
                UpdateSymbolTable(node.Left, prefix);
                prefix.Remove(prefix.Length - 1, 1);

                // traverse right
                prefix.Append("1"); // 1
                UpdateSymbolTable(node.Right, prefix);
                prefix.Remove(prefix.Length - 1, 1);
            }
        }

        public string Encode(string stringToEncode)
        {
            StringBuilder encodedString = new StringBuilder();
            foreach (char c in stringToEncode)
                encodedString.Append(SymbolTable[c]);
            return encodedString.ToString();
        }
    }
}
