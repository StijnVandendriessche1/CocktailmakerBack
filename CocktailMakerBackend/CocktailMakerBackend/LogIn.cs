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
    public static class LogIn
    {
        [FunctionName("LogIn")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req, ILogger log)
        {
            try
            {
                string constr = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
                string json = await new StreamReader(req.Body).ReadToEndAsync();
                User trial = JsonConvert.DeserializeObject<User>(json);
                User user = new User();
                bool succes = false;

                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = constr;
                    await con.OpenAsync();
                    using(SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "select * from tbl_user where name = @name";
                        cmd.Parameters.AddWithValue("@name", trial.name);
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        if(reader.HasRows)
                        {
                            while(reader.Read())
                            {
                                user.ID = reader["ID"].ToString();
                                user.name = reader["name"].ToString();
                                user.password = reader["password"].ToString();
                                user.role = reader["role"].ToString();
                                user.machine_id = reader["machine_id"].ToString();
                                if (trial.PasswordHash == user.password)
                                {
                                    succes = true;
                                    user.password = null;
                                }
                            }
                        }
                    }
                }
                if(succes)
                {
                    using(SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = constr;
                        await con.OpenAsync();
                        using(SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandText = "update tbl_user set session_id = @session_id, session_time = @session_time where ID = @ID";
                            user.session_id = Guid.NewGuid().ToString();
                            cmd.Parameters.AddWithValue("@session_id", user.session_id);
                            user.session_time = GetTimestamp(DateTime.Now);
                            cmd.Parameters.AddWithValue("@session_time", user.session_time);
                            cmd.Parameters.AddWithValue("@ID", user.ID);
                            await cmd.ExecuteNonQueryAsync();
                            return new OkObjectResult(user);
                        }
                    }
                }
                return new OkObjectResult("{\"result\":\"fail\"}");
            }
            catch(Exception ex)
            {
                log.LogError(ex + "        --------> Log In");
                return new StatusCodeResult(500);
            }
        }

        public static String GetTimestamp(DateTime value)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp.ToString();
        }
    }
}
