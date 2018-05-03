using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProfWillow.Entities
{
    public class TelegramMessage
    {
        [JsonProperty(PropertyName = "location")]
        public Location Location { get; set; }
    }

    public class Location
    { 
        [JsonProperty(PropertyName = "latitude")]
        public float Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public float Longitude { get; set; }
    }
}