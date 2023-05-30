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

        private void Sample(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var r = _clt.ReadHoldingRegisters(_id, 0x3100, 28);

            double pvVoltage = BitConverter.ToInt16(r) / 100.0; // 0x3100, 1/100 V
            double pvCurrent = BitConverter.ToInt16(r.Slice(1)) / 100.0; // 0x3101, 1/100 A
            double pvPower = BitConverter.ToInt32(r.Slice(2)) / 100.0; // 0x3102, 1/100 W 32 bit

            double batteryVoltage = BitConverter.ToInt16(r.Slice(4)) / 100.0; // 0x3104, 1/100V
            double batteryCurrent = BitConverter.ToInt16(r.Slice(5)) / 100.0; // 0x3105, 1/100A
            double batteryPower = BitConverter.ToInt32(r.Slice(6)) / 100.0; // 0x3106, 1/100 W 32 bit

            double loadVoltage = BitConverter.ToInt16(r.Slice(0xC)) / 100.0; // 0x310C, 1/100V
            double loadCurrent = BitConverter.ToInt16(r.Slice(0xD)) / 100.0; // 0x310D, 1/100A
            double loadPower = BitConverter.ToInt32(r.Slice(0xF)) / 100.0; // 0x310F, 1/100 W 32 bit

            double batteryTemp = BitConverter.ToInt16(r.Slice(0x10)) / 100.0; // 0x3110, 1/100 °C
            double deviceTemp = BitConverter.ToInt16(r.Slice(0x11)) / 100.0; // 0x3111, 1/100 °C

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
