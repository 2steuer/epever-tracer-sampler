using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver.Receivers
{
    internal class EpeverSampleHandler : IMessageReceiver
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        private SampleWriterDelegate _writer;

        private string _measurementName;

        public EpeverSampleHandler(SampleWriterDelegate writer, string measurementName)
        {
            _writer = writer;
            _measurementName = measurementName ?? string.Empty;
        }

        public void HandleMessage(string payload)
        {
            _log.Trace($"Sample received!");

            try
            {
                var sample = DataSample.FromJson(payload);

                _writer(this, _measurementName, sample!); // sample should not be null here, otherwise throw NullReferenceException
            }
            catch (Exception e)
            {
                _log.Error(e, "Error while processing sample");
            }
        }
    }
}
