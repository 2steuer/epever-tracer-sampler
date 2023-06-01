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



var mqttOpt = MqttOptions.FromConfig(cfg.GetSection("Mqtt"));

var mqtt = new SamplerMqttHandler(mqttOpt);

solar.NewSample += mqtt.SendSample;

await mqtt.Start();
solar.Start();

TaskCompletionSource cancelSource = new TaskCompletionSource();
bool haveSigInt = false;

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("Processing SIGINT");
    eventArgs.Cancel = true;
    haveSigInt = true;
    cancelSource.TrySetResult();
};

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    if (!haveSigInt)
    {
        Console.WriteLine("Processing SIGTERM");
        cancelSource.TrySetResult();
    }
    else
    {
       Console.WriteLine($"Got SIGTERM but ignoring it because of SIGINT before");
    }
};

await cancelSource.Task;

solar.Stop();
await mqtt.Stop();