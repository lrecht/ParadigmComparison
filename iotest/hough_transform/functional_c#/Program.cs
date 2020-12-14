using System;
using System.Drawing;
using System.Linq;
using System.Collections.Immutable;

namespace functional_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var pic = new Bitmap("iotest/hough_transform/Pentagon.png");
            System.Console.WriteLine(pic.Width);
        }
    }
}
