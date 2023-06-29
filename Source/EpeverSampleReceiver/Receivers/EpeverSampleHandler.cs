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

        public EpeverSampleHandler(SampleWriterDelegate writer)
        {
            _writer = writer;
        }

        public void HandleMessage(string payload)
        {
            _log.Trace($"Sample received!");

            try
            {
                var sample = DataSample.FromJson(payload);

                _writer(this, sample!); // sample should not be null here, otherwise throw NullReferenceException
            }
            catch (Exception e)
            {
                _log.Error(e, "Error while processing sample");
            }
        }
    }
}
