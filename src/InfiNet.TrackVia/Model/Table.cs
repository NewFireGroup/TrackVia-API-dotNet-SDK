using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfiNet.TrackVia.Model
{
    public class Table
    {
        public Table()
        {
            Fields = new List<TableFieldMetadata>();
        }

        
        [JsonProperty("fields")]
        public List<TableFieldMetadata> Fields{ get; set; }

        [JsonProperty("id")]
        public long? Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("identifierTemplate")]
        public string IdentifierTemplate { get; set; }
        [JsonProperty("itemLabel")]
        public string ItemLabel { get; set; }
        [JsonProperty("itemLabelPlural")]
        public string ItemLabelPlural { get; set; }

        [JsonProperty("created")]
        public DateTime? Created { get; set; }
        [JsonProperty("updated")]
        public DateTime? Updated { get; set; }

        // Posting Variables
        [JsonProperty("dirty")]
        public bool? Dirty { get; set; }

        [JsonProperty("parsedIdentifierTemplate")]
        public string ParsedIdentifierTemplate { get; set; }

        [JsonProperty("tempIdentifierTemplate")]
        public string TempIdentifierTemplate { get; set; }

        [JsonProperty("appId")]
        public long? AppId { get; set; }
    }
}
