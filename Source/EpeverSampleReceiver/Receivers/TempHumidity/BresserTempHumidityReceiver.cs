using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver.Receivers.TempHumidity
{
    internal class BresserTempHumidityReceiver : IMessageReceiver
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private SampleWriterDelegate _writer;

        private Dictionary<int, string> _channels;

        public BresserTempHumidityReceiver(SampleWriterDelegate writer, Dictionary<int, string> channels)
        {
            _writer = writer;
            _channels = channels;
        }

        public void HandleMessage(string payload)
        {
            try
            {
                var d = JsonConvert.DeserializeObject<TempHumiditySample>(payload);

                if (!_channels.ContainsKey(d.Channel))
                {
                    _log.Warn("Received temperature data with undefined channel.");
                    return;
                }

                //d.Timestamp = DateTime.SpecifyKind(d.Timestamp, DateTimeKind.Local);

                Dictionary<string, double> v = new Dictionary<string, double>();
                v.Add("temperature", d.TempC);
                v.Add("humidity", d.Humidity);

                _writer(this, "climate", new DataSample(d.Timestamp, v), ("location", _channels[d.Channel]));
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error while processing temp/humidity message");
            }
        }
    }
}
