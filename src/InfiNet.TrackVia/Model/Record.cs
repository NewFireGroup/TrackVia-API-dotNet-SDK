using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class Record
    {
        public RecordData Data { get; set; }
        public List<FieldMetadata> Structure { get; set; }

        public Record()
        {
            Structure = new List<FieldMetadata>();
            Data = new RecordData();
        }

        public Record(IEnumerable<FieldMetadata> structure, RecordData data)
        {
            Structure = new List<FieldMetadata>(structure);
            Data = new RecordData(data);
        }

        #region override Equals

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Record)) return false;

            Record otherRecord = (Record)obj;

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
