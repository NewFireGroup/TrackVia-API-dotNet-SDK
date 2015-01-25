using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class RecordDataBatch
    {
        public RecordDataBatch()
        {
            this.Data = new List<RecordData>();
        }

        public RecordDataBatch(IEnumerable<RecordData> data)
        {
            this.Data = new List<RecordData>(data);
        }

        [Newtonsoft.Json.JsonProperty("data")]
        public List<RecordData> Data { get; set; }
    }
}
