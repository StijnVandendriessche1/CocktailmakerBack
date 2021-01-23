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

namespace CocktailMakerBackend
{
    public static class GetMachineName
    {
        [FunctionName("GetMachineName")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "machine/{id}/name")] HttpRequest req, int id,ILogger log)
        {
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionstring;
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "select model from tbl_machine where ID = @ID;";
                        cmd.Parameters.AddWithValue("@ID", id);
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                return new OkObjectResult(reader["model"].ToString());
                            }
                        }
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
