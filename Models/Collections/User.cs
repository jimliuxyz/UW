using Newtonsoft.Json;

namespace UW.Models.Collections
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string userId { get; set; }

        public string name { get; set; }

        public string phoneno { get; set; }
    }
}