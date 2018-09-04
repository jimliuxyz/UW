using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Models.Collections
{
    // public enum @stringx { CNY, USD, BTC, ETH }

    public class Balance //snapshot
    {
        [JsonProperty(PropertyName = "id")]
        public string ownerId { get; set; }

        public List<BalanceSlot> balances { get; set; }
    }
    public class BalanceSlot
    {
        public string name { get; set; }
        public string balance { get; set; }
    }
}
