using Newtonsoft.Json;

namespace TracerSamplerCommon
{
    public class DataSample
    {
        public DateTime TimeStamp { get; set; }

        public Dictionary<string, double> Data { get; set; } = null!;

        public DataSample()
        {
            // this constructor is here to make the json converter do its work!
        }

        public DataSample(DateTime timeStamp, Dictionary<string, double> data)
        {
            TimeStamp = timeStamp;
            Data = data;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static DataSample? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<DataSample>(json);
        }
    }
}