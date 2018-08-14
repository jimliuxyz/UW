using Newtonsoft.Json;

namespace UW.Models.Collections
{
    public class SmsPasscode
    {
        [JsonProperty(PropertyName = "id")]
        public string phoneno { get; set; }
        public string passcode { get; set; }
    }
}
