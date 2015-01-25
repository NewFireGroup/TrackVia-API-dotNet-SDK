using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class RecordSet
    {
        [Newtonsoft.Json.JsonIgnore()]
        public int Count { get { return this.Data != null ? this.Data.Count : 0; } }

        public RecordSet()
        {
            this.Structure = new List<FieldMetadata>();
            this.Data = new List<RecordData>();
        }
        public RecordSet(IEnumerable<FieldMetadata> structure, IEnumerable<RecordData> data)
        {
            this.Structure = new List<FieldMetadata>(structure);
            this.Data = new List<RecordData>(data);
        }

        [Newtonsoft.Json.JsonProperty("data")]
        public List<RecordData> Data { get; set; }

        public List<FieldMetadata> Structure { get; set; }
    }
}
