// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using NLog;
using System.Drawing;
using BatteryMonitorSampler;
using EpeverTracerSampler.Sampler;
using TracerSamplerCommon;

ILogger _log = LogManager.GetCurrentClassLogger();

_log.Info("Starting application...");

var cfg = new ConfigurationBuilder()
    .AddJsonFile(args[0])
    .Build();

var port = cfg.GetValue<string>("Monitor:Port")!;
var baud = cfg.GetValue<int>("Monitor:Baudrate");

var mqttOpt = MqttOptions.FromConfig(cfg.GetSection("Mqtt"));

var mqtt = new SamplerMqttHandler(mqttOpt);
mqtt.SampleTopic = "battery-monitor";
await mqtt.Start();

var mon = new BatteryMonitorHandler(port, baud);
mon.Start();

mon.NewSample += mqtt.SendSample;

_log.Info("Application started.");

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

// stop everything
await mon.Stop();
await mqtt.Stop();

_log.Info("Shutdown complete.");
