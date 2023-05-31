using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TracerSamplerCommon
{
    public class MqttHandler
    {
        private MqttOptions _opt;

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
            });

            var c = new MqttFactory().CreateManagedMqttClient();
            await c.StartAsync(ob.Build());
            _client = c;

            await StartActions();
            Running = true;
        }

        public async Task Stop()
        {
            await StopActions();
            await Client.StopAsync();
            Running = false;
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
