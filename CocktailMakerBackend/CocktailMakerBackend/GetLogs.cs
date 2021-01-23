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
    public static class GetLogs
    {
        [FunctionName("GetLogs")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "logs/{param}")] HttpRequest req, string param, ILogger log)
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
                log.LogError(ex + "        --------> SetRead/check loggin");
                return new StatusCodeResult(500);
            }
            try
            {
                if (loged_in)
                {
                    List<Log> logs = new List<Log>();
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = connectionstring;
                        await con.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            if(param == "all")
                            {
                                cmd.CommandText = "select * from tbl_logboek where user_id = @ID;";
                            }
                            else if(param == "new")
                            {
                                cmd.CommandText = "select * from tbl_logboek where user_id = @ID and [read] = 0;";
                            }
                            else
                            {
                                return new OkObjectResult("{\"result\":\"fail\"}");
                            }
                            cmd.Parameters.AddWithValue("@ID", trial.ID);
                            SqlDataReader reader = await cmd.ExecuteReaderAsync();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Log l = new Log();
                                    l.ID = reader["ID"].ToString();
                                    l.message = reader["message"].ToString();
                                    l.mode = reader["mode"].ToString();
                                    logs.Add(l);
                                }
                            }
                        }
                    }
                    return new OkObjectResult(logs);
                }
                return new OkObjectResult("{\"result\":\"fail\"}");
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> SetRead/set read");
                return new StatusCodeResult(500);
            }
        }

    }
}
