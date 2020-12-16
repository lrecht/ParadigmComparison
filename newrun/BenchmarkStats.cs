using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace newrun
{
    public class BenchmarkStats
    {
        public Dictionary<PerformanceMetrics, MetricStats> performanceMetrics;

        public BenchmarkStats()
        {
            performanceMetrics = new Dictionary<PerformanceMetrics, MetricStats>();
            performanceMetrics.Add(PerformanceMetrics.ExecutionTime, new MetricStats("run_time.csv", "run_time", "µs"));
            //performanceMetrics.Add(PerformanceMetrics.Package, new MetricStats("pkg_power.csv", "pkg_power", "µj"));
            //performanceMetrics.Add(PerformanceMetrics.RAM, new MetricStats("ram_power.csv", "ram_power", "µj"));
        }

        public void Clear()
        {
            foreach (MetricStats stats in performanceMetrics.Values)
                stats.Clear();
        }

        public void AddResult(double[] result)
        {
            int i = 0;
            foreach (MetricStats stats in performanceMetrics.Values)
                stats.AddMeasurement(result[i++]);
        }

        public void ComputeResults()
        {
            foreach (MetricStats stats in performanceMetrics.Values)
                stats.ComputeResult();
        }

        public void SaveResults(string benchmarkName)
        {
            foreach (MetricStats stats in performanceMetrics.Values)
                stats.SaveToFile(benchmarkName);
        }

        public class MetricStats
        {
            public double Mean = 0, StDev = 0, ErrorMargin = 0, ErrorPercent = 0;
            List<double> Measurements { get; set; }
            string ouputFilePath { get; set; }
            string name { get; set; }
            string unit { get; set; }

            public MetricStats(string outputFilePath, string name, string unit)
            {
                Measurements = new List<double>();
                this.ouputFilePath = outputFilePath;
                this.name = name;
                this.unit = unit;
                
            }
            public void Clear()
            {
                (Mean, StDev, ErrorMargin, ErrorPercent) = (0, 0, 0, 0);
                Measurements.Clear();
            }

            public void AddMeasurement(double measure) => Measurements.Add(measure);

            public void ComputeResult()
            {
                Mean = computeMean();
                StDev = computeStDev();
                ErrorMargin = computeErrorMargin();
                ErrorPercent = computeErrorPercent();
            }

            public void SaveToFile(string benchmarkName)
            {
                using (StreamWriter w = File.AppendText(ouputFilePath))
                {
                    string line = String.Join(";", new string[] {benchmarkName, Mean.ToString(), StDev.ToString(), ErrorMargin.ToString(), ErrorPercent.ToString(), Measurements.Count.ToString()});
                    w.WriteLine(line);
                }
            }

            double computeMean() => Measurements.Sum() / Measurements.Count;
            double computeStDev()
            {
                double avg = Measurements.Average();
                return Math.Sqrt(Measurements.Average(v => Math.Pow(v - avg, 2)));
            }

            double computeErrorMargin() => StDev / Math.Sqrt(Measurements.Count);
            double computeErrorPercent() => ErrorMargin / Mean;
        }
    }
}