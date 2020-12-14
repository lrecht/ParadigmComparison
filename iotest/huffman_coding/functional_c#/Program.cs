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
            var TEST_STRING = File.ReadAllText("iotest/huffman_coding/lines.txt");
            System.Console.WriteLine(TEST_STRING.Length);
        }
    }
}
