using System;
using System.Linq;
using System.Collections.Immutable;

namespace functional_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var edgesRep = System.IO.File.ReadAllLines("iotest/spanning_tree/graph.csv")
                            .Select(l => l.Split(','))
                            .Select((val, i) => (i, Convert.ToInt32(val[0]), Convert.ToInt32(val[1]), Convert.ToInt32(val[2])));

            System.Console.WriteLine(edgesRep.Count());
        }
    }
}
