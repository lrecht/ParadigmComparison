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
            var pic = new Bitmap("iotest/canny_edge_detector/download.jpg");
            System.Console.WriteLine(pic.Width);
        }
    }
}
