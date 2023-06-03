using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using NLog;

namespace TracerSamplerCommon
{
    public class MqttHandler
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private MqttOptions _opt;

        protected MqttOptions Options => _opt;

        public bool Running { get; set; } = false;

        private IManagedMqttClient? _client = null;

        protected IManagedMqttClient Client { get
            {
                if (_client == null)
                {
                    throw new ArgumentException("Client is null");
                }

                return _client;
            } 
        }

        public MqttHandler(MqttOptions options) 
        {
            _opt = options;
        }

        public async Task Start()
        {
            ManagedMqttClientOptionsBuilder ob = new ManagedMqttClientOptionsBuilder();

            ob.WithClientOptions(co =>
            {
                co.WithTcpServer(_opt.Host, _opt.Port);
                co.WithClientId(_opt.ClientId);
                if (_opt.Authenticate)
                {
                    co.WithCredentials(_opt.User, _opt.Password);
                }

                if (_opt.PublishState)
                {
                    /*co.WithWillMessage(_opt.StateTopic)
                        .WithWillPayload(_opt.StateOffPayload)
                        .WithWillRetain(true);*/

                    co.WithWillMessage(new MqttApplicationMessageBuilder()
                        .WithTopic(_opt.StateTopic)
                        .WithPayload(_opt.StateOffPayload)
                        .WithRetainFlag()
                        .Build());
                }
            });

            _log.Info($"Connecting to MQTT On {_opt.Host}:{_opt.Port}...");

            var c = new MqttFactory().CreateManagedMqttClient();
            await c.StartAsync(ob.Build());
            _client = c;

            await _client.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(_opt.StateTopic)
                .WithRetainFlag(true)
                .WithPayload(_opt.StateOnPayload)
                .Build());

            _client.UseConnectedHandler(ConnectedHandler);
            _client.UseDisconnectedHandler(DisconnectedHandler);

            _log.Debug($"Performing start actions");
            await StartActions();
            Running = true;

            _log.Info($"MQTT Connection started.");
        }

        private Task DisconnectedHandler(MqttClientDisconnectedEventArgs arg)
        {
            _log.Info("MQTT Disconnected");
            return Task.CompletedTask;
        }

        private Task ConnectedHandler(MqttClientConnectedEventArgs arg)
        {
            _log.Info("MQTT Connected");
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            _log.Info($"Stopping MQTT handling");
            await StopActions();

            await Client.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(_opt.StateTopic)
                .WithRetainFlag(true)
                .WithPayload(_opt.StateOffPayload)
                .Build());

            await Task.Delay(TimeSpan.FromMilliseconds(250));
            
            await Client.StopAsync();

            _client = null;
            Running = false;

            _log.Info($"MQTT Stopped");
        }

        protected virtual Task StartActions()
        {
            return Task.CompletedTask;
        }

        protected virtual Task StopActions()
        {
            return Task.CompletedTask;
        }
    }
}
