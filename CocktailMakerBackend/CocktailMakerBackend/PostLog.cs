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
    public static class PostLog
    {
        [FunctionName("PostLog")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addlogs/{machine_id}")] HttpRequest req, string machine_id, ILogger log)
        {
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            string json = await new StreamReader(req.Body).ReadToEndAsync();
            Log logrecord = JsonConvert.DeserializeObject<Log>(json);
            List<string> ids = new List<string>();
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionstring;
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "select * from tbl_user where machine_id = @ID";
                        cmd.Parameters.AddWithValue("@ID", machine_id);
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ids.Add(reader["ID"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> Get Name");
                return new StatusCodeResult(500);
            }
            try
            {
                foreach(string i in ids)
                {
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = connectionstring;
                        await con.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandText = "INSERT INTO cocktailmakerdb.dbo.tbl_logboek (user_id, message, [read], mode) VALUES(@user_id, @message, 0, @mode);";
                            cmd.Parameters.AddWithValue("@user_id", i);
                            cmd.Parameters.AddWithValue("@message", logrecord.message);
                            cmd.Parameters.AddWithValue("@mode", logrecord.mode);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return new OkObjectResult("done");
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> Get Name");
                return new StatusCodeResult(500);
            }
        }
    }
}
