using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class DomainRecord<T>
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public T Data { get; set; }
        [Newtonsoft.Json.JsonProperty("structure")]
        public List<FieldMetadata> Structure { get; set; }

        public DomainRecord()
        {
            Structure = new List<FieldMetadata>();
        }

        public DomainRecord(IEnumerable<FieldMetadata> structure, T data)
        {
            Structure = new List<FieldMetadata>(structure);
            Data = data;
        }

        #region override Equals

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is DomainRecord<T>)) return false;

            DomainRecord<T> otherRecord = (DomainRecord<T>)obj;

            if(this.Data == null || otherRecord.Data == null)
                return false;

            return this.Data.Equals(otherRecord.Data); 
        }

        public override int GetHashCode()
        {
            return this.Data != null ? this.Data.GetHashCode() : 0;
        }

        #endregion
    }
}
