using System;
using System.Diagnostics;
using System.Linq;

namespace newrun
{
    public static class SampleSize
    {
        public static int ComputeCochran(BenchmarkStats statsObject)
        {
            // Compute Mean, StDev, ErrorMargin, and ErrorPercent
            statsObject.ComputeResults();
            int[] numRuns = new int[3];
            double zScore = 1.96; // For 95 % confidence
            int i = 0;
            foreach(var metric in statsObject.performanceMetrics.Values)
            {
                double numerator = zScore * metric.StDev;
                double error = metric.Mean * 0.005;
                numRuns[i++] = (int)Math.Ceiling(Math.Pow((numerator / error),2));
            }
            return numRuns.Max();
        }
    }
}