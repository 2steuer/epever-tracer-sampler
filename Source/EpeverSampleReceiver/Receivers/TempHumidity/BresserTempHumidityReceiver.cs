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
        private string _locationTag;

        public BresserTempHumidityReceiver(SampleWriterDelegate writer, string locationTag)
        {
            _writer = writer;
            _locationTag = locationTag;
        }

        public void HandleMessage(string payload)
        {
            try
            {
                var d = JsonConvert.DeserializeObject<TempHumiditySample>(payload);
                d.Timestamp = DateTime.SpecifyKind(d.Timestamp, DateTimeKind.Local);

                Dictionary<string, double> v = new Dictionary<string, double>();
                v.Add("temperature", d.TempC);
                v.Add("humidity", d.Humidity);

                _writer(this, new DataSample(d.Timestamp, v), ("location", _locationTag));
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error while processing temp/humidity message");
            }
        }
    }
}
