using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentModbus;
using TracerSamplerCommon;
using Timer = System.Timers.Timer;

namespace EpeverTracerSampler.Sampler
{
    internal class TracerSampler
    {
        public event Action<object, TracerSample>? NewSample; 

        private readonly ModbusRtuClient _clt;

        private readonly string _port;
        private readonly byte _id;

        private readonly Timer _timer = new();

        public TracerSampler(string port, int baudrate, byte id, int sampleIntervalMs)
        {
            _port = port;
            _id = id;

            _clt = new ModbusRtuClient()
            {
                BaudRate = baudrate,
                Handshake = Handshake.None,
                Parity = Parity.None,
                StopBits = StopBits.One
            };

            _timer.Interval = sampleIntervalMs;
            _timer.AutoReset = true;
            _timer.Elapsed += Sample;
        }

        private int DecodeDword(short l, short h)
        {
            return ((ushort)l | (h << 16));
        }

        private void Sample(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Sampling...");

            try
            {
                var r = _clt.ReadInputRegisters<short>(_id, 0x3100, 18);

                double pvVoltage = r[0x00] / 100.0; // 0x3100, 1/100 V
                double pvCurrent = r[0x01] / 100.0; // 0x3101, 1/100 A
                double pvPower = DecodeDword(r[0x02], r[0x03]) / 100.0; // 0x3102, 1/100 W 32 bit

                double batteryVoltage = r[0x04] / 100.0; // 0x3104, 1/100V
                double batteryCurrent = r[0x05] / 100.0; // 0x3105, 1/100A
                double batteryPower = DecodeDword(r[0x06], r[0x07]) / 100.0; // 0x3106, 1/100 W 32 bit

                double loadVoltage = r[0x0C] / 100.0; // 0x310C, 1/100V
                double loadCurrent = r[0x0D] / 100.0; // 0x310D, 1/100A
                double loadPower = DecodeDword(r[0x0E], r[0x0F]) / 100.0; // 0x310F, 1/100 W 32 bit

                double batteryTemp = r[0x10] / 100.0; // 0x3110, 1/100 °C
                double deviceTemp = r[0x11] / 100.0; // 0x3111, 1/100 °C

                var s = new TracerSample(DateTime.Now, new()
            {
                {"solarVoltage", pvVoltage},
                {"solarCurrent", pvCurrent},
                {"solarPower", pvPower},
                {"batteryVoltage", batteryVoltage},
                {"batteryCurrent", batteryCurrent},
                {"batteryPower", batteryPower},
                {"loadVoltage", loadVoltage},
                {"loadCurrent", loadCurrent},
                {"loadPower", loadPower},
                {"batteryTemperature", batteryTemp},
                {"deviceTemperature", deviceTemp}
            });

                NewSample?.Invoke(this, s);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Start()
        {
            _clt.Connect(_port, ModbusEndianness.LittleEndian);

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();

            if (_clt.IsConnected)
            {
                _clt.Close();
            }
        }
    }
}
