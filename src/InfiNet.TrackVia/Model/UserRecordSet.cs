using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class UserRecordSet
    {
        public UserRecordSet()
        {
            this.Structure = new List<FieldMetadata>();
            this.Data = new List<User>();
        }

        public UserRecordSet(IEnumerable<FieldMetadata> structure, IEnumerable<User> data)
        {
            this.Structure = new List<FieldMetadata>(structure);
            this.Data = new List<User>(data);
        }

        public List<FieldMetadata> Structure { get; set; }
        public List<User> Data { get; set; }

        public int Count { get { return this.Data != null ? this.Data.Count : 0; } }
    }
}
