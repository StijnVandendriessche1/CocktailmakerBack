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
    public static class GetCocktails
    {
        [FunctionName("GetCocktails")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cocktails")] HttpRequest req, ILogger log)
        {
            bool loged_in = false;
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            string json = await new StreamReader(req.Body).ReadToEndAsync();
            User trial = JsonConvert.DeserializeObject<User>(json);
            try
            {
                using(SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionstring;
                    await con.OpenAsync();
                    using(SqlCommand cmd = new SqlCommand())
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
                log.LogError(ex + "        --------> Get Cockatails/check loggin");
                return new StatusCodeResult(500);
            }
            try
            {
                if(loged_in)
                {
                    List<Cocktail> cocktails = new List<Cocktail>();
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = connectionstring;
                        await con.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandText = "select * from tbl_cocktail;";
                            SqlDataReader reader = await cmd.ExecuteReaderAsync();
                            while(reader.Read())
                            {
                                Cocktail c = new Cocktail();
                                c.ID = reader["ID"].ToString();
                                c.name = reader["name"].ToString();
                                c.code = reader["code"].ToString();
                                cocktails.Add(c);
                            }
                        }
                    }
                    return new OkObjectResult(cocktails);
                }
                return new OkObjectResult("FAIL");
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> Get Cockatails/get cocktails");
                return new StatusCodeResult(500);
            }
        }
    }
}
