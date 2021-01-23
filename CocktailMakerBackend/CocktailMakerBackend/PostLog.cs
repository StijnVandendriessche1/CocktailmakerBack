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

namespace CocktailMakerBackend
{
    public static class PostLog
    {
        [FunctionName("PostLog")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "logs/add")] HttpRequest req, ILogger log)
        {
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionstring;
                    string json = await new StreamReader(req.Body).ReadToEndAsync();
                    Log logrecord = JsonConvert.DeserializeObject<Log>(json);
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "INSERT INTO cocktailmakerdb.dbo.tbl_logboek (user_id, message, [read], mode) VALUES(@user_id, @message, 0, @mode);";
                        cmd.Parameters.AddWithValue("@user_id", logrecord.user_id);
                        cmd.Parameters.AddWithValue("@message", logrecord.message);
                        cmd.Parameters.AddWithValue("@mode", logrecord.mode);
                        await cmd.ExecuteNonQueryAsync();
                        return new OkObjectResult("done");
                    }
                }
                return new OkObjectResult("unidentified");
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> Get Name");
                return new StatusCodeResult(500);
            }
        }
    }
}
