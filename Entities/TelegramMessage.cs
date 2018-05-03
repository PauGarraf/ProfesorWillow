using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProfWillow.Entities
{
    public class TelegramMessage
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
        [JsonProperty(PropertyName = "parameters")]
        public Parameters Parameters { get; set; }
    }

    public class Parameters
    {
        [JsonProperty(PropertyName = "chat_id")]
        public string ChatId { get; set; }
        [JsonProperty(PropertyName = "latitute")]
        public float? Latitute { get; set; }
        [JsonProperty(PropertyName = "longitute")]
        public float? Longitute { get; set; }
    }
}