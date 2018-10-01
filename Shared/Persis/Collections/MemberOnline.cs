using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Shared.Persis.Collections
{
    public class MemberOnline
    {
        [JsonProperty(PropertyName = "id")]
        public string memId { get; set; }
        public string pk { get; set; }
        public long createdTime = DateTime.UtcNow.ToFileTimeUtc();

        public string offlineTokenHash { get; set; } // hash = hash(sys_part+user_part)

        public decimal offlineSpent = 0;
    }

    //TTL : 8 hours
}
