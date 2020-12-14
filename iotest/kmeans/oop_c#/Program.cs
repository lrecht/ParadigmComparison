using System;
using System.IO;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        private static Point[] points;
        static void Main(string[] args)
        {
            points = File.ReadAllLines($"iotest/kmeans/points.txt")
                                .Select(l => new Point(Convert.ToDouble(l.Split(':')[0]), Convert.ToDouble(l.Split(':')[1])))
                                .ToArray();
            System.Console.WriteLine(points.Length);
        }
    }

    public class Point
    {
        public readonly double X, Y;
        public Point(double x, double y) => (X, Y) = (x, y);
    }
}
