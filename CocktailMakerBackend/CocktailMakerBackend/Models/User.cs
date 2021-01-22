using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CocktailMakerBackend.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "ID")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string role { get; set; }

        [JsonProperty(PropertyName = "machine_id")]
        public string machine_id { get; set; }

        [JsonProperty(PropertyName = "session_id")]
        public string session_id { get; set; }

        [JsonProperty(PropertyName = "session_time")]
        public string session_time { get; set; }

        public string PasswordHash
        {
            get
            {
                return GetStringSha256Hash(password);
            }
        }

        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

    }
}
