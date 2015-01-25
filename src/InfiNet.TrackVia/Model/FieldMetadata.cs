using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class FieldMetadata
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public bool Unique { get; set; }
        public List<string> Choices { get; private set; }
        public bool CanRead { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanCreate { get; set; }

        public FieldMetadata()
        {
            this.Choices = new List<string>();
        }

        public FieldMetadata(string name, string type, bool required, bool unique, IEnumerable<string> choices) : this()
        {
            this.Name = name;
            this.Type = type;
            this.Required = required;
            this.Unique = unique;

            this.Choices.AddRange(choices);
        }
    }
}
