﻿// See https://aka.ms/new-console-template for more information

using InfluxDB.Client;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Drawing;
using EpeverSampleReceiver;
using TracerSamplerCommon;
using EpeverSampleReceiver = EpeverSampleReceiver.Receivers.EpeverSampleHandler;
using EpeverSampleReceiver.Receivers;
using EpeverSampleReceiver.Receivers.Rain;
using EpeverSampleReceiver.Receivers.Soil;
using EpeverSampleReceiver.Receivers.TempHumidity;
using System.Configuration;

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

mqtt.AddHandler("/samples", new EpeverSampleHandler(db.Write, "solar"));
mqtt.AddHandler("/battery-monitor", new EpeverSampleHandler(db.Write, "battery"));
mqtt.AddHandler(cfg.GetValue<string>("RainSensorTopic") ?? throw new ArgumentException("Rainfall sensor topic not configured."), new TfaDropReceiver(db.Write));

var tempHumTopic = cfg.GetValue<string>("TempHum:Topic") ?? throw new ConfigurationErrorsException("Temp Hum Topic not given.");
var tempHumChannels = cfg.GetChannelMapping("TempHum:Channels");

mqtt.AddHandler(tempHumTopic, new BresserTempHumidityReceiver(db.Write, tempHumChannels));


var xh300topic = cfg.GetValue<string>("XH300:Topic") ?? throw new ConfigurationErrorsException("XH300 topic not given.");
var xh300channels = cfg.GetChannelMapping("XH300:Channels");


mqtt.AddHandler(xh300topic, new XH300Receiver(db.Write, xh300channels));

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