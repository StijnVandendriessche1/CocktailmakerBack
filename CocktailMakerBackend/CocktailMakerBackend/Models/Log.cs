using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CocktailMakerBackend.Models
{
    class Log
    {
        [JsonProperty(PropertyName = "ID")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string user_id { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string message { get; set; }

        [JsonProperty(PropertyName = "read")]
        public bool read { get; set; }

        [JsonProperty(PropertyName = "mode")]
        public string mode { get; set; }
    }
}
