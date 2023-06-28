using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using TracerSamplerCommon;

namespace EpeverTracerSampler.Sampler
{
    internal class SamplerMqttHandler : MqttHandler
    {
        public SamplerMqttHandler(MqttOptions options) : base(options)
        {
        }

        public async void SendSample(object? sender, DataSample sample)
        {
            var msg = new ManagedMqttApplicationMessageBuilder()
                .WithApplicationMessage(new MqttApplicationMessageBuilder()
                    .WithTopic($"{Options.BaseTopic}/samples")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithPayload(sample.ToJson())
                    .WithContentType("application/json")
                    .Build())
                .Build();

            await Client.PublishAsync(msg);
        }
    }
}
