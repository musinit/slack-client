using Newtonsoft.Json;

namespace Slack.Client.Models.SlackApi
{
    public class ProfileResponse : SlackResponse
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }
        [JsonProperty("profile")]
        public Profile Profile { get; set; }
    }
}