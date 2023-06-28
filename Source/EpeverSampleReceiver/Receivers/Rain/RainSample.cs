using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EpeverSampleReceiver.Receivers.Rain
{
    internal class RainSample
    {
        [JsonProperty("time")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("rain_mm")]
        public double RaimMillimeters { get; set; }
    }
}
