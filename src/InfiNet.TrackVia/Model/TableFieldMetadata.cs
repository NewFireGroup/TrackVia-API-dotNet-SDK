using Newtonsoft.Json;
using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    public class TableFieldMetadata
    {
        public TableFieldMetadata()
        {
            this.Choices = new List<string>();
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("required")]
        public bool? Required { get; set; }
        [JsonProperty("unique")]
        public bool? Unique { get; set; }

        [JsonProperty("discriminatorType")]
        public string DiscriminatorType { get; set; }
        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }
        [JsonProperty("partOfRecordId")]
        public bool? PartOfRecordId { get; set; }
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("formula")]
        public string Formula { get; set; }

        [JsonProperty("choices")]
        public List<string> Choices { get; set; }

        // Edit Schema Fields (possibly unneeded
        [JsonProperty("gridColWidth")]
        public int? GridColWidth { get; set; }
        [JsonProperty("gridEditable")]
        public bool? GridEditable { get; set; }
        [JsonProperty("gridTemplate")]
        public string GridTemplate { get; set; }
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        //public List<string> Choices { get; private set; }
    }
}
