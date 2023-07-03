using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EpeverSampleReceiver.Receivers.TempHumidity
{
    internal class TempHumiditySample
    {
        [JsonProperty("time")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("temperature_F")]
        public double TempF { get; set; }

        [JsonIgnore] 
        public double TempC => (TempF - 32) * 5 / 9;

        [JsonProperty("humidity")]
        public double Humidity { get; set; }

    }
}
