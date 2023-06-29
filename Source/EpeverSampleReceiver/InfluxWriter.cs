using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using NLog;
using TracerSamplerCommon;

namespace EpeverSampleReceiver
{
    internal class InfluxWriter
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private readonly InfluxDBClient _db;

        private string _dbName;

        public InfluxWriter(string url, string user, string password, string org, string token, string database)
        {
            var opt = new InfluxDBClientOptions(url)
            {
                Username = user,
                Password = password,
                Org = org,
                Token = token,
                Bucket = database
            };

            _db = new InfluxDBClient(opt);
            
            _dbName = database;
        }

        public async void Write(object? sender, DataSample sample, params (string tag, string value)[] tags)
        {
            List<PointData> mmts = new List<PointData>();

            foreach (var d in sample.Data)
            {
                var s = PointData.Measurement(d.Key)
                    .Field("value", d.Value)
                    .Timestamp(sample.TimeStamp, WritePrecision.Ms);

                foreach (var valueTuple in tags)
                {
                    s.Tag(valueTuple.tag, valueTuple.value);
                }

                mmts.Add(s);
            }

            var wrt = _db.GetWriteApiAsync();
            try
            {
                if (mmts.Count == 1)
                {
                    await wrt.WritePointAsync(mmts[0], _dbName);
                }
                else if (mmts.Count > 1)
                {
                    await wrt.WritePointsAsync(mmts, _dbName);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Error while writing samples to Influx");
            }
        }

    }
}
