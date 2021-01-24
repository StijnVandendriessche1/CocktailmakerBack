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
using Microsoft.Azure.Devices;
using System.Collections.Generic;

namespace CocktailMakerBackend
{
    public static class SetName
    {
        [FunctionName("SetName")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "machine/setname/{name}")] HttpRequest req, string name, ILogger log)
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
                                    trial.machine_id = reader["machine_id"].ToString();
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
                if (loged_in)
                {
                    if(name.Length > 16)
                    {
                        return new OkObjectResult("{\"result\":\"fail\"}");
                    }
                    using (SqlConnection con = new SqlConnection())
                    {
                        con.ConnectionString = connectionstring;
                        await con.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandText = "update tbl_machine set model = @name where ID = @ID";
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@ID", trial.machine_id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                else
                {
                    return new OkObjectResult("{\"result\":\"fail\"}");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> Get Cockatails/get cocktails");
                return new StatusCodeResult(500);
            }
            try
            {
                if (!String.IsNullOrEmpty(trial.machine_id))
                {
                    ServiceClient sCli;
                    string iotconstr = Environment.GetEnvironmentVariable("IOTHubMainConnectionstring");
                    sCli = ServiceClient.CreateFromConnectionString(iotconstr);
                    CloudToDeviceMethod method = new CloudToDeviceMethod("name");
                    Dictionary<string, string> msg = new Dictionary<string, string>
                    {
                        { "name", name }
                    };
                    string data = JsonConvert.SerializeObject(msg, Formatting.Indented);
                    method.SetPayloadJson(data);
                    CloudToDeviceMethodResult r = await sCli.InvokeDeviceMethodAsync("testpi", method);
                    log.LogInformation(r.GetPayloadAsJson());
                    return new OkObjectResult("{\"result\":\"succes\"}");
                }
                else
                {
                    return new OkObjectResult("{\"result\":\"no machine\"}");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex + "        --------> MakeCocktail/send DirectMethod");
                return new StatusCodeResult(500);
            }
        }
    }
}
