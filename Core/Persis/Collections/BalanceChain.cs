using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Core.Persis.Collections
{
    public class BalanceChain
    {
        [JsonProperty(PropertyName = "id")]
        public decimal sn { get; set; } //sequence number
        public string snhash { get; set; }  //hash(sn, 當下內容, hash[1])
        public string txRequestId { get; set; }
        public string txReceiptId { get; set; }

        public string userId { get; set; }
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime datetime { get; set; }

        public bool outflow { get; set; }
        public decimal amount { get; set; }
        public decimal balance { get; set; } //snapshot
    }

}
