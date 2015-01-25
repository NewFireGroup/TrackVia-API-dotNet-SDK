using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class DomainRecordSet<T>
    {
        public int Count { get { return this.Data != null ? this.Data.Count : 0; } }

        public DomainRecordSet()
        {
            this.Structure = new List<FieldMetadata>();
            this.Data = new List<T>();
        }
        public DomainRecordSet(IEnumerable<FieldMetadata> structure, IEnumerable<T> data)
        {
            this.Structure = new List<FieldMetadata>(structure);
            this.Data = new List<T>(data);
        }

        public List<T> Data { get; set; }
        public List<FieldMetadata> Structure { get; set; }
    }
}
