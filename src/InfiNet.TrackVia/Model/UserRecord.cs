using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class UserRecord
    {
        public UserRecord()
        {
            this.Structure = new List<FieldMetadata>();
            this.Data = new User();
        }

        public UserRecord(List<FieldMetadata> structure, User data)
        {
            this.Structure = new List<FieldMetadata>(structure);
            this.Data = data;
        }

        public List<FieldMetadata> Structure { get; set; }
        public User Data { get; set; }
    }
}
