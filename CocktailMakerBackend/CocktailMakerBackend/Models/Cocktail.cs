using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;

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

        public async Task<string> getRecipe()
        {
            string recipe = "";
            List<List<string[]>> steps = getSteps(code);
            foreach(List<string[]> i in steps)
            {
                foreach(string[] j in i)
                {
                    string n = await getDrink(Convert.ToInt32(j[0]));
                    recipe += String.Format("{0} parts of {1}\n", j[1], n);
                }
            }
            return recipe;
        }

        internal static List<List<string[]>> getSteps(string code)
        {
            code.ToUpper();
            List<List<string[]>> output = new List<List<string[]>>();
            string[] parts = code.Split('X');
            foreach(string p in parts)
            {
                List<string[]> between = new List<string[]>();
                string[] steps = p.Split("N");
                foreach(string s in steps)
                {
                    between.Add(s.Split("Q"));
                }
                output.Add(between);
            }
            return output;
        }

        private async Task<string> getDrink(int id)
        {
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = connectionstring;
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = "select name from tbl_drink where pomp = @pomp;";
                    cmd.Parameters.AddWithValue("@pomp", id);
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return reader["name"].ToString();
                        }
                    }
                }
            }
            return "not found";
        }
    }
}
