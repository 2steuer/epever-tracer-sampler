using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver.Receivers.Rain
{
    internal class TfaDropReceiver : IMessageReceiver
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private SampleWriterDelegate _writer;

        public TfaDropReceiver(SampleWriterDelegate writer)
        {
            _writer = writer;
        }

        public void HandleMessage(string payload)
        {
            _log.Debug("Sample received.");

            try
            {
                var s = JsonConvert.DeserializeObject<RainSample>(payload);
                s.Timestamp = DateTime.SpecifyKind(s.Timestamp, DateTimeKind.Local);

                _log.Debug($"Timestamp: {s.Timestamp}, Rainfall: {s.RaimMillimeters}");

                Dictionary<string, double> dic = new Dictionary<string, double>();
                dic.Add("rainfall_mm", s.RaimMillimeters);

                var ds = new DataSample(s.Timestamp, dic);

                _writer(this, "rain", ds);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error while handling JSON.");
            }
        }
    }
}
