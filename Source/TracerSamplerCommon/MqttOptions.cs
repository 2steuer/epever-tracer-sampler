using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
