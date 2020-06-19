using Newtonsoft.Json;

namespace Slack.Client.Models.SlackApi
{
    public class ListUserIdsResponse : SlackResponse
    {
        [JsonProperty("members")]
        public string[] SlackUserIds { get; set; }
        [JsonProperty("response_metadata")]
        public ResponseMetadata Metadata { get; set; }
    }
}
