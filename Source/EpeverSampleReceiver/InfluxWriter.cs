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
using LogLevel = InfluxDB.Client.Core.LogLevel;

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

        public async void Write(object? sender, string measurementName, DataSample sample, params (string tag, string value)[] tags)
        {
            PointData point = PointData.Measurement(measurementName)
                .Timestamp(sample.TimeStamp, WritePrecision.Ms);

            foreach (var d in sample.Data)
            {
                point = point.Field(d.Key, d.Value);
                    

                foreach (var valueTuple in tags)
                {
                    point = point.Tag(valueTuple.tag, valueTuple.value);
                }
            }
            
            var wrt = _db.GetWriteApiAsync();
            
            try
            {
                await wrt.WritePointAsync(point, _dbName);
            }
            catch (Exception e)
            {
                _log.Error(e, "Error while writing samples to Influx");
            }
        }

    }
}
