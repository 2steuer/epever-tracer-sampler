﻿// See https://aka.ms/new-console-template for more information

using InfluxDB.Client;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Drawing;
using EpeverSampleReceiver;
using TracerSamplerCommon;

ILogger _log =  LogManager.GetCurrentClassLogger();

_log.Info("Starting application");

var cfg = new ConfigurationBuilder()
    .AddJsonFile(args[0])
    .Build();

var mqttOpt = MqttOptions.FromConfig(cfg.GetSection("Mqtt"));

var mqtt = new MqttSampleReceiver(mqttOpt);

var db = new InfluxWriter(
    cfg.GetValue<string>("Influx:Url", string.Empty)!,
    cfg.GetValue<string>("Influx:User", string.Empty)!,
    cfg.GetValue<string>("Influx:Password", string.Empty)!,
    cfg.GetValue<string>("Influx:Organization", string.Empty)!,
    cfg.GetValue<string>("Influx:Token", string.Empty)!,
    cfg.GetValue<string>("Influx:Database", string.Empty)!
);

mqtt.NewSample += db.Write;

await mqtt.Start();

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

await mqtt.Stop();

_log.Info("Shutdown complete.");