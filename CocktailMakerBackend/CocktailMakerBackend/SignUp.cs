using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CocktailMakerBackend.Models;
using System.Data.SqlClient;

namespace CocktailMakerBackend
{
    public static class SignUp
    {
        [FunctionName("SignUp")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "signup")] HttpRequest req,ILogger log)
        {
            string constr = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            string json = await new StreamReader(req.Body).ReadToEndAsync();
            User trial = JsonConvert.DeserializeObject<User>(json);
            bool succes = false;
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = constr;
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "select * from tbl_user where name = @name";
                        cmd.Parameters.AddWithValue("@name", trial.name);
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            succes = false;
                        }
                        else
                        {
                            succes = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> Sign Up/check username");
                return new StatusCodeResult(500);
            }
            try
            {
                if(succes)
                {
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = constr;
                        await con.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            if (String.IsNullOrEmpty(trial.machine_id))
                            {
                                cmd.CommandText = "insert into tbl_user (name, password, role) values (@name, @password, 'user')";
                            }
                            else
                            {
                                cmd.CommandText = "insert into tbl_user (name, password, role, machine_id) values (@name, @password, 'user', @machine_id)";
                                cmd.Parameters.AddWithValue("@machine_id", Convert.ToInt32(trial.machine_id));
                            }
                            cmd.Parameters.AddWithValue("@password", trial.PasswordHash);
                            cmd.Parameters.AddWithValue("@name", trial.name);
                            await cmd.ExecuteNonQueryAsync();
                            return new OkObjectResult("{\"result\":\"succes\"}");
                        }
                    }
                }
                return new OkObjectResult("{\"result\":\"fail\"}");
            }
            catch(Exception ex)
            {
                log.LogError(ex + "        --------> Sign Up/insert user");
                return new StatusCodeResult(500);
            }
        }
    }
}
