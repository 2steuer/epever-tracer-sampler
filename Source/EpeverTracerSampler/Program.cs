// See https://aka.ms/new-console-template for more information
using EpeverTracerSampler.Sampler;
using Microsoft.Extensions.Configuration;
using TracerSamplerCommon;

Console.WriteLine("Starting...");

var cfg = new ConfigurationBuilder()
    .AddJsonFile(args[0])
    .Build();

var solar = new TracerSampler(
        cfg.GetValue<string>("Charger:Port")!,
        cfg.GetValue<int>("Charger:Baudrate"),
        cfg.GetValue<byte>("Charger:DeviceId"),
        cfg.GetValue<int>("Charger:SampleInterval")
    );

var mqttOpt = new MqttOptions
{
    Host = cfg.GetValue<string>("Mqtt:Host"),
    Port = cfg.GetValue<int>("Mqtt:Port"),
    BaseTopic = cfg.GetValue<string>("Mqtt:BaseTopic"),
    Authenticate = cfg.GetValue<bool>("Mqtt:Authenticate"),
    User = cfg.GetValue<string>("Mqtt:User", string.Empty),
    Password = cfg.GetValue<string>("Mqtt:Password", string.Empty),
    ClientId = cfg.GetValue<string>("Mqtt:ClientId", Guid.NewGuid().ToString())
};

//solar.NewSample += NewSampleHandler;

solar.Start();

