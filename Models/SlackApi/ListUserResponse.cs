﻿using Newtonsoft.Json;

namespace Slack.Client.Models.SlackApi
{
    public class ListUserResponse : SlackResponse
    {
        [JsonProperty("members")]
        public SlackUser[] Users { get; set; }
        [JsonProperty("response_metadata")]
        public ResponseMetadata Metadata { get; set; }
    }

    public class ResponseMetadata
    {
        [JsonProperty("next_cursor")]
        public string NextCursor { get; set; }
    }
}
