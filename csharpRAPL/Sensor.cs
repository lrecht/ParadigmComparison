using System;
using System.Linq;
using System.Collections.Generic;

public enum CollectionApproach
{
    AVERAGE,
    DIFFERENCE
}

namespace csharpRAPL
{
    public class Sensor
    {
        public string Name { get; }
        private DeviceAPI _api;
        private CollectionApproach _approach;
        private List<double> startValue;
        private List<double> endValue;

        public Sensor(string name, DeviceAPI api, CollectionApproach approach)
        {
            Name = name;
            _api = api;
            _approach = approach;
        }

        public void Start()
        {
            startValue = _api.Energy();
        }

        public void End()
        {
            endValue = _api.Energy();
        }

        public List<double> Delta()
        {
            switch (_approach)
            {
                case CollectionApproach.DIFFERENCE:
                    return Enumerable.Range(0, endValue.Count).Select(i => endValue[i] - startValue[i]).ToList();
                case CollectionApproach.AVERAGE:
                    return Enumerable.Range(0, endValue.Count).Select(i => (endValue[i] + startValue[i]) / 2).ToList();
                default:
                    throw new Exception("Collection approach is not available");
            }
        }
    }
}
