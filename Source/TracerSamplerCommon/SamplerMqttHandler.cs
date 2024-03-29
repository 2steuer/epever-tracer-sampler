﻿using System;
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
    public class SamplerMqttHandler : MqttHandler
    {
        public string SampleTopic { get; set; } = "samples";

        public SamplerMqttHandler(MqttOptions options) : base(options)
        {
        }

        public async void SendSample(object? sender, DataSample sample)
        {
            var msg = new ManagedMqttApplicationMessageBuilder()
                .WithApplicationMessage(new MqttApplicationMessageBuilder()
                    .WithTopic($"{Options.BaseTopic}/{SampleTopic}")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithPayload(sample.ToJson())
                    .WithContentType("application/json")
                    .Build())
                .Build();

            await Client.PublishAsync(msg);
        }
    }
}
