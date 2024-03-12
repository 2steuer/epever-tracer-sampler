using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using TracerSamplerCommon;

namespace BatteryMonitorSampler
{
    internal class BatteryMonitorHandler
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        public event Action<object?, DataSample> NewSample; 

        public string Port { get; set; }

        public int Baudrate { get; set; }

        private SerialPort _port;
        private Thread _runner;

        private TaskCompletionSource<bool> _exitTcs;
        private CancellationTokenSource _exitCts;

        public BatteryMonitorHandler(string port, int baudrate)
        {
            Port = port;
            Baudrate = baudrate;

            _port = new SerialPort(Port, Baudrate);
            _port.ReadTimeout = 10000;
        }

        public void Start()
        {
            _port.Open();

            _exitTcs = new TaskCompletionSource<bool>();
            _exitCts = new CancellationTokenSource();

            _runner = new Thread(Handler);
            _runner.IsBackground = true;
            _runner.Start();
        }

        public async Task Stop()
        {
            _exitCts.Cancel();

            await _exitTcs.Task;

            _port.Close();
        }

        private void Handler()
        {
            while (!_exitCts.IsCancellationRequested)
            {
                try
                {
                    var l = _port.ReadLine();

                    var parts = l.Split('\t');

                    var busVoltage = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    var current = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    var power = double.Parse(parts[2], CultureInfo.InvariantCulture);

                    DataSample sample = new DataSample(DateTime.Now, new()
                    {
                        { "busVoltage", busVoltage },
                        { "busCurrent", current },
                        { "busPower", power }
                    });

		    NewSample?.Invoke(this, sample);
                }
                catch (TimeoutException)
                {
                    _log.Warn("Reading from serial port timed out!");
                }
                catch (FormatException e)
                {
                    _log.Warn(e, "Format exception while handling data");
                }
                catch (Exception e)
                {
                    _log.Error(e, "Unhandled exception");
                    throw;
                }
            }

            _exitTcs.TrySetResult(true);
        }
    }
}
