using System.Collections.Generic;
using Newtonsoft.Json;

namespace UW.Shared.Persis.Collections
{
    public class Contacts
    {
        [JsonProperty(PropertyName = "id")]
        public string ownerId { get; set; }

        public List<string> friendOf { get; set; }
        public List<Friend> friends { get; set; }
    }

    public class Friend
    {
        public string userId { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
    }
}
