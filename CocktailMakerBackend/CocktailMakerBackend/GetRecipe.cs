using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using CocktailMakerBackend.Models;
using System.Collections.Generic;

namespace CocktailMakerBackend
{
    public static class GetRecipe
    {
        [FunctionName("GetRecipe")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recipe/{id}")] HttpRequest req, string id, ILogger log)
        {
            bool loged_in = false;
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            string json = await new StreamReader(req.Body).ReadToEndAsync();
            User trial = JsonConvert.DeserializeObject<User>(json);
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionstring;
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "select * from tbl_user where ID = @ID";
                        cmd.Parameters.AddWithValue("@ID", trial.ID);
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string session_id = reader["session_id"].ToString();
                                if (trial.session_id == session_id)
                                {
                                    loged_in = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> GetRecipe/check loggin");
                return new StatusCodeResult(500);
            }
            try
            {
                if (loged_in)
                {
                    Cocktail c = new Cocktail();
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = connectionstring;
                        await con.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandText = "select * from tbl_cocktail where ID = @ID;";
                            cmd.Parameters.AddWithValue("@ID", id);
                            SqlDataReader reader = await cmd.ExecuteReaderAsync();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    c.ID = id;
                                    c.name = reader["name"].ToString();
                                    c.code = reader["code"].ToString();
                                }
                            }
                        }
                    }
                    string r = await c.getRecipe();
                    return new OkObjectResult("{\"recipe\":\"" + r + "\"}");
                }
                return new OkObjectResult("{\"result\":\"fail\"}");
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> GetRecipe/get recipe");
                return new StatusCodeResult(500);
            }
        }
    }
}
