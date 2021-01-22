using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CocktailMakerBackend.Models
{
    class Cocktail
    {
        [JsonProperty(PropertyName = "ID")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string code { get; set; }
    }
}
