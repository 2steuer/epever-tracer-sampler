using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpeverSampleReceiver.Receivers;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using MQTTnet.Server.Internal;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver
{
    public delegate void SampleWriterDelegate(object? sender, string measurementName, DataSample sample, params (string tag, string value)[] tags);

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

            _log.Info($"Subscribing to {t}");

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
            var handler = _handlers
                .FirstOrDefault(kvp => MqttTopicFilterComparer.IsMatch(arg.ApplicationMessage.Topic, kvp.Key));

            if (handler.Value == null)
            {
                return Task.CompletedTask;
            }

            var pl = arg.ApplicationMessage.ConvertPayloadToString();

            handler.Value.HandleMessage(pl);

            return Task.CompletedTask;
        }
    }
}
