using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Shared.Persis.Collections
{
    public class TxLocker
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string memId { get; set; }
        public string pk { get; set; }
    }
}
