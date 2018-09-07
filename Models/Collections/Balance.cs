using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Models.Collections
{
    public class Balance //snapshot
    {
        [JsonProperty(PropertyName = "id")]
        public string ownerId { get; set; }

        public List<BalanceSlot> balances { get; set; }
    }
    public class BalanceSlot
    {

        // currency name
        public string name { get; set; }
        public string balance { get; set; }
    }
}
