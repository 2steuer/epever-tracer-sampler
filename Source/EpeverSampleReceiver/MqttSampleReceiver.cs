using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpeverSampleReceiver.Receivers;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver
{
    public delegate void SampleWriterDelegate(object? sender, DataSample sample, params (string tag, string value)[] tags);

    internal class MqttSampleReceiver : MqttHandler
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private Dictionary<string, IMessageReceiver> _handlers = new();

        public MqttSampleReceiver(MqttOptions options) : base(options)
        {

        }

        public void AddHandler(string topic, IMessageReceiver messageReceiver)
        {
            var t = topic.StartsWith("/") ? $"{Options.BaseTopic}{topic}" : topic;

            _handlers.Add(t, messageReceiver);
        }

        protected override async Task StartActions()
        {
            foreach (var messageReceiver in _handlers)
            {
                var f = new MqttTopicFilterBuilder()
                    .WithTopic(messageReceiver.Key).Build();

                await Client.SubscribeAsync(new[] { f });
            }

            Client.UseApplicationMessageReceivedHandler(MessageReceived);
        }

        private Task MessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (!_handlers.ContainsKey(arg.ApplicationMessage.Topic))
            {
                return Task.CompletedTask;
            }

            var pl = arg.ApplicationMessage.ConvertPayloadToString();

            _handlers[arg.ApplicationMessage.Topic].HandleMessage(pl);

            return Task.CompletedTask;
        }
    }
}
