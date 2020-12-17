using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace csharpRAPL
{
    public class RAPL
    {
        private List<Sensor> apis = new List<Sensor>{
            new Sensor("pkg", new PackageAPI(), CollectionApproach.DIFFERENCE), 
            new Sensor("dram", new DramAPI(), CollectionApproach.DIFFERENCE),
            new Sensor("temp", new TempAPI(), CollectionApproach.AVERAGE)
        };

        private Stopwatch sw = new Stopwatch();
        private TimeSpan elapsedTime;
        
        public void Start() 
        {
            sw.Reset();
            apis.ForEach(api => api.Start());
            sw.Start();
        }

        public void End()
        {
            sw.Stop();
            apis.ForEach(api => api.End());
            elapsedTime = sw.Elapsed;
        }

        public bool IsValid() => apis.All(api => api.IsValid());

        //Not general. Only returns one result as our pcs have one socket
        public (TimeSpan, List<(string deviceName, double energyUsed)>) GetResult()
        {
            var results = apis.Select(api => (api.Name, api.Delta[0])).ToList();
            return (elapsedTime, results);
        }
    }
}
