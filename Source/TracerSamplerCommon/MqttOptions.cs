using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TracerSamplerCommon
{
    public class MqttOptions
    {
        public string? Host { get; set; }
        public int Port { get; set; }

        public string? BaseTopic { get; set; }

        public bool Authenticate { get; set; }

        public string? User { get; set; }

        public string? Password { get; set; }

        public string? ClientId { get; set; }

        public bool PublishState { get; set; } = false;

        public string? StateTopic { get; set; }

        public string? StateOnPayload { get; set; } = "Online";

        public string? StateOffPayload { get; set; } = "Offline";

        public static MqttOptions FromConfig(IConfigurationSection cfg)
        {
            var mqttOpt = new MqttOptions
            {
                Host = cfg.GetValue<string>("Host"),
                Port = cfg.GetValue<int>("Port"),
                BaseTopic = cfg.GetValue<string>("BaseTopic"),
                Authenticate = cfg.GetValue<bool>("Authenticate"),
                User = cfg.GetValue<string>("User", string.Empty),
                Password = cfg.GetValue<string>("Password", string.Empty),
                ClientId = cfg.GetValue<string>("ClientId", Guid.NewGuid().ToString()),
                PublishState = cfg.GetValue<bool>("State:PublishState", false),
                StateTopic = cfg.GetValue<string>("State:Topic", string.Empty),
                StateOnPayload = cfg.GetValue<string>("State:OnPayload", string.Empty),
                StateOffPayload = cfg.GetValue<string>("State:OffPayload", string.Empty)
            };

            return mqttOpt;
        }
    }
}
