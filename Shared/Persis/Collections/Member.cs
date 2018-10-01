using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Shared.Persis.Collections
{
    public class Member
    {
        [JsonProperty(PropertyName = "id")]
        public string memId { get; set; }
        public string pk { get; set; }

        public long createdTime { get; set; }
        public string alias { get; set; }   //Unique Key in whole collection

        // public bool allowDiscover = true;
        public string name { get; set; }

        public string phoneno { get; set; }

        public string avatar { get; set; }
        public string tokenRnd { get; set; }    //用來驗證會員最後建立的JWT

        public List<CurrencySettings> currencies { get; set; }

        public NtfInfo ntfInfo { get; set; }
    }
    
}
