// See https://aka.ms/new-console-template for more information
using EpeverTracerSampler.Sampler;
using Microsoft.Extensions.Configuration;
using NLog;
using TracerSamplerCommon;

ILogger _log = LogManager.GetCurrentClassLogger();

_log.Info("Starting application...");

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

_log.Info("Application started");

TaskCompletionSource cancelSource = new TaskCompletionSource();
bool haveSigInt = false;

Console.CancelKeyPress += (sender, eventArgs) =>
{
    _log.Debug("Processing SIGINT");
    eventArgs.Cancel = true;
    haveSigInt = true;
    cancelSource.TrySetResult();
};

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    if (!haveSigInt)
    {
        _log.Debug("Processing SIGTERM");
        cancelSource.TrySetResult();
    }
    else
    {
       _log.Debug($"Got SIGTERM but ignoring it because of SIGINT before");
    }
};

await cancelSource.Task;

_log.Info("Stopping application");

solar.Stop();
await mqtt.Stop();

_log.Info("Shutdown complete.");