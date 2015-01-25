using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class DomainRecordDataBatch<T>
    {
        public DomainRecordDataBatch()
        {
            Data = new List<T>();
        }
        public DomainRecordDataBatch(IEnumerable<T> data)
        {
            Data = new List<T>(data);
        }

        [Newtonsoft.Json.JsonProperty("data")]
        public List<T> Data { get; set; }
    }
}
