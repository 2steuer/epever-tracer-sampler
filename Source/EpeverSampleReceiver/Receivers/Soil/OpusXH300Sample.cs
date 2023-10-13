using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EpeverSampleReceiver.Receivers.Rain
{
    internal class OpusXH300Sample
    {
        [JsonProperty("time")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("temperature_C")]
        public double Temperature { get; set; }

        [JsonProperty("moisture")]
        public double Moisture { get; set; }
    }
}
