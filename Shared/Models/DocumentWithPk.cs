using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace UW.Shared.Misc
{
    public class DocumentWithPk : Document
    {
        [JsonProperty(PropertyName = "pk")]
        public string pk { get; set; }
    }
}