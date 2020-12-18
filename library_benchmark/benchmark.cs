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
        static readonly int maxExecutionTime = 2700; //In seconds
        static readonly string outputFilePath = "tempResults.csv";
        int iterations { get; }
        double elapsedTime = 0;
        List<Measure> _resultBuffer = new List<Measure>();
        RAPL _rapl;
        TextWriter stdout;
        StreamWriter blackHole = new StreamWriter(Stream.Null); // Prints everything to a null stream similar to /dev/null


        public Benchmark(int iterations) 
        {
            //Sets console to write to null
            this.stdout = System.Console.Out;
            System.Console.SetOut(blackHole);

            this.iterations = iterations;
            this._rapl = new RAPL();
        }


        private void start() 
        {
            _rapl.Start();
        }


        private void end()
        {
            _rapl.End();

            //Result only valid if all results are valid
            //Only then is the result added and duration is incremented
            if (_rapl.IsValid()) {
                Measure mes = new Measure(_rapl.GetResult());
                _resultBuffer.Add(mes);
                elapsedTime += mes.duration.TotalSeconds;
            } 
        }
        

        // Used to run benchmarks which take a single input argument -- The benchmarks is curried into a function which takes zero input arguments
        public void Run<I, R>(Func<I, R> benchmark, I input, Action<R> benchmarkOutput) => Run(() => benchmark(input), benchmarkOutput);


        //Performns benchmarking
        //Writes progress to stdout if there is more than one iteration
        public void Run<R>(Func<R> benchmark, Action<R> benchmarkOutput) 
        {
            elapsedTime = 0;
            _resultBuffer = new List<Measure>();
            for (int i = 0; i < iterations; i++)
            {
                if(iterations != 1)
                    print(System.Console.Write, $"\r{i + 1} of {iterations}");
                
                //Actually performing benchmark and resulting IO
                start();
                R res = benchmark();
                end();
                benchmarkOutput(res);
                
                if (elapsedTime >= maxExecutionTime){
                    print(System.Console.WriteLine, "\nEnding benchmark due to time constraints");
                    break;
                }
            }
    
            if (iterations != 1)
                print(System.Console.WriteLine);
                
            saveResults();
        }


        /// Used to print to standard out -- Everything printed outside this method will not be shown
        private void print(Action<string> printAction, string value = "")
        {
            System.Console.SetOut(stdout);
            printAction(value);
            System.Console.Out.Flush();
            System.Console.SetOut(blackHole);
        }

        //Saves result to temporary file
        //This is overwritten each time SaveResults is run
        private void saveResults()
        {
            var header = "duration(ms);pkg(µj);dram(µj);temp(C)" + "\n";
            string result = header;
            
            foreach (Measure m in _resultBuffer)
            {
                result += m.duration.TotalMilliseconds;
                foreach (var res in m.apis)
                {
                    //Temperature api
                    if (res.apiName.Equals("temp"))
                        result += ";" + ((double)res.apiValue / 1000);
                    //All other apis
                    else
                        result += ";" + res.apiValue;
                }
                result += "\n";
            }

            File.WriteAllText(outputFilePath, result);
        }
    }
}
