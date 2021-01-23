using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CocktailMakerBackend.Models
{
    class Drink
    {
        [JsonProperty(PropertyName = "ID")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "alcohol")]
        public int alcohol { get; set; }

        [JsonProperty(PropertyName = "pomp")]
        public int pomp { get; set; }
    }
}
