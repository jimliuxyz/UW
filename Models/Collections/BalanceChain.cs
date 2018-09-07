using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Models.Collections
{
    //decimal max : 79228162514264337593543950335
    //ulong max   : 18446744073709551615

    public class BalanceChain
    {
        [JsonProperty(PropertyName = "id")]
        public decimal sn { get; set; } //serial number
        public string snhash { get; set; }  //hash(sn, 當下內容, hash[1])
        public string txRequestId { get; set; }
        public string txReceiptId { get; set; }

        public string userId { get; set; }
        public DateTime datetime { get; set; }

        public bool outflow { get; set; }
        public decimal amount { get; set; }
        public decimal balance { get; set; } //??
    }

}
