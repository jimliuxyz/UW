using System.Collections.Generic;
using Newtonsoft.Json;

namespace UW.Shared.Persis.Collections
{
    public class Contacts
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string pk { get; set; }
        public string userId { get; set; }  //unique key
        public int vol { get; set; }  //unique key

        public List<string> friendOf { get; set; }  //怎麼限制數量?
        public List<Friend> friends { get; set; }
    }

    public class Friend
    {
        public string userId { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
    }
}
