using System.Collections.Generic;
using System.Diagnostics;

namespace csharpRAPL.devices {
    public class TimerAPI : DeviceAPI {
        private Stopwatch sw = new Stopwatch();
        
        public TimerAPI() {
            sw.Start();
        }

        public override List<string> openRAPLFiles() => null;

        public override List<double> Collect(){
            return new List<double>() {sw.Elapsed.TotalMilliseconds};
        }
    }
}
