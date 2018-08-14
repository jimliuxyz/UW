using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UW.Models.Collections
{
    public enum PNS { gcm, apns }

    public class NoHubInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Owner { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PNS Pns { get; set; } //Push Notification System

        public string PnsRegId { get; set; }

        public string AzureRegId { get; set; }
    }
}
