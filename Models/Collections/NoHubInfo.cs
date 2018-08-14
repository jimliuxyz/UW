using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Models.Collections
{
    public enum PNS { gcm, apns }

    public class NoHubInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string ownerId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PNS pns { get; set; } //Push Notification System

        public string pnsRegId { get; set; }

        public string azureRegId { get; set; }
    }
}
