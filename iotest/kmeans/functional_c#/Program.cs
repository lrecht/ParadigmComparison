using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace functional_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var points = System.IO.File.ReadAllLines("iotest/kmeans_concurrent/points.txt")
                .Select(x => (Convert.ToDouble(x.Split(':')[0]), Convert.ToDouble(x.Split(':')[1])))
                .ToImmutableList();
            System.Console.WriteLine(points.Count);
        }
    }
}
