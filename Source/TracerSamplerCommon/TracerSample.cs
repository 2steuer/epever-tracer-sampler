using Newtonsoft.Json;

namespace TracerSamplerCommon
{
    public class TracerSample
    {
        public DateTime TimeStamp { get; set; }

        public Dictionary<string, double> Data { get; set; } = null!;

        public TracerSample()
        {
            // this constructor is here to make the json converter do its work!
        }

        public TracerSample(DateTime timeStamp, Dictionary<string, double> data)
        {
            TimeStamp = timeStamp;
            Data = data;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static TracerSample? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<TracerSample>(json);
        }
    }
}