using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver
{
    internal class MqttSampleReceiver : MqttHandler
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        public event Action<object, TracerSample>? NewSample; 

        private string _sampleTopic;

        public MqttSampleReceiver(MqttOptions options) : base(options)
        {
            _sampleTopic = Options.BaseTopic + "/samples";
        }

        protected override async Task StartActions()
        {
            var f = new MqttTopicFilterBuilder()
                .WithTopic(_sampleTopic).Build();

            await Client.SubscribeAsync(new[] {f});

            Client.UseApplicationMessageReceivedHandler(MessageReceived);
        }

        private Task MessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (arg.ApplicationMessage.Topic != _sampleTopic)
            {
                return Task.CompletedTask;
            }

            _log.Debug($"Sample received!");

            var json = arg.ApplicationMessage.ConvertPayloadToString();

            try
            {
                var sample = TracerSample.FromJson(json);

                NewSample?.Invoke(this, sample!); // sample should not be null here, otherwise throw NullReferenceException
            }
            catch (Exception e)
            {
                _log.Error(e, "Error while processing sample");
            }

            return Task.CompletedTask;
        }
    }
}
