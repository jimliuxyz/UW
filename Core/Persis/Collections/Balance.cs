using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Core.Persis.Collections
{
    public class Balance //snapshot (this may replace with Redis)
    {
        [JsonProperty(PropertyName = "id")]
        public string userId { get; set; }

        public Dictionary<string, decimal> balances { get; set; }
    }
    public class BalanceSlot
    {

        // currency name
        public string name { get; set; }
        public string balance { get; set; }
    }
}
