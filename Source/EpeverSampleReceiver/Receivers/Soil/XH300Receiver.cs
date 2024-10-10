using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using EpeverSampleReceiver.Receivers.Rain;
using Newtonsoft.Json;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver.Receivers.Soil
{
    internal class XH300Receiver : IMessageReceiver
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private SampleWriterDelegate _writer;

        private Dictionary<int, string> _locationByChannel;

        private Dictionary<int, DateTime> _lastSamples = new Dictionary<int, DateTime>();

        private TimeSpan _minInterval = TimeSpan.FromSeconds(10);

        public XH300Receiver(SampleWriterDelegate writer, Dictionary<int, string> locationByChannel)
        {
            _writer = writer;
            _locationByChannel = locationByChannel;
        }

        public void HandleMessage(string payload)
        {
            _log.Trace("Sample received");

            try
            {
                var s = JsonConvert.DeserializeObject<OpusXH300Sample>(payload);
                s.Timestamp = DateTime.SpecifyKind(s.Timestamp, DateTimeKind.Local);

                if (!_lastSamples.ContainsKey(s.Channel))
                {
                    _lastSamples.Add(s.Channel, DateTime.MinValue);
                }

                if (s.Timestamp - _lastSamples[s.Channel] <= _minInterval)
                {
                    return;
                }

                _lastSamples[s.Channel] = s.Timestamp;

                var location = _locationByChannel.ContainsKey(s.Channel) ? _locationByChannel[s.Channel] : "unknown";

                Dictionary<string, double> dic = new Dictionary<string, double>
                {
                    { "temperature", s.Temperature },
                    { "moisture", s.Moisture }
                };

                var ds = new DataSample(s.Timestamp, dic);

                _writer(this, "soil-data", ds, ("location", location));
            }
            catch (Exception e)
            {
                _log.Error(e, "Error while processing sample");
            }
        }
    }
}
