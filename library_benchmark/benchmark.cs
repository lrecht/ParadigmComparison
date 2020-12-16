using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using csharpRAPL;

struct Measure
{
    public TimeSpan duration;
    public List<(string apiName, double apiValue)> apis;
    
    public Measure((TimeSpan, List<(string, double)>) raplResult){
        this.duration = raplResult.Item1;
        this.apis = raplResult.Item2;
    }
}

namespace benchmark
{
    public class Benchmark
    {
        static readonly string outputFilePath = "tempResults.csv";
        int iterations { get; }
        List<Measure> _resultBuffer = new List<Measure>();
        RAPL _rapl;

        public Benchmark(int iterations) 
        {
            this.iterations = iterations;
            this._rapl = new RAPL();
        }

        public void Start() 
        {
            _rapl.Start();
        }

        public void End()
        {
            _rapl.End();
            _resultBuffer.Add(new Measure(_rapl.getResult()));
        }

        public void Run<I, R>(Func<I, R> benchmark, I input, Action<R> print) 
        {
            _resultBuffer = new List<Measure>();
            for (int i = 0; i < iterations; i++)
            {
                Start();
                R res = benchmark(input);
                End();
                print(res);
            }
            SaveResults();
        }

        public void Run<R>(Func<R> benchmark, Action<R> print) 
        {
            _resultBuffer = new List<Measure>();
            for (int i = 0; i < iterations; i++)
            {
                Start();
                R res = benchmark();
                End();
                print(res);
            }
            SaveResults();
        }

        public void SaveResults()
        {
            var header = "duration(ms);pkg(µj);dram(µj);temp(C)" + "\n";
            string result = header;
            
            foreach (Measure m in _resultBuffer)
            {
                result += m.duration.TotalMilliseconds;
                foreach (var res in m.apis)
                {
                    if (res.apiName.Equals("temp"))
                        result += ";" + ((double)res.apiValue / 1000);
                    else
                        result += ";" + res.apiValue;
                }
                result += "\n";
            }

            File.WriteAllText(outputFilePath, result);
        }
    }
}
