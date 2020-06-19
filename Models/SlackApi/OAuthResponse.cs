using Newtonsoft.Json;

namespace Slack.Client.Models.SlackApi
{
    public class OAuthResponse : SlackResponse
    {
        [JsonProperty("authed_user")]
        public AuthedUser AuthedUser { get; set; }
        [JsonProperty("access_token")]
        public string BotAccessToken { get; set; }
        [JsonProperty("team")]
        public TeamResponse Team { get; set; }
    }

    public class TeamResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    
    public class AuthedUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
