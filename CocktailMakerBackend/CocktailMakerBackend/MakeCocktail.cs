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
    public static class MakeCocktail
    {
        [FunctionName("MakeCocktail")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "make/{id}")] HttpRequest req, string id, ILogger log)
        {
            bool loged_in = false;
            string connectionstring = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            string json = await new StreamReader(req.Body).ReadToEndAsync();
            User trial = JsonConvert.DeserializeObject<User>(json);
            Cocktail cocktail = new Cocktail();
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
                log.LogError(ex + "        --------> Make Cocktail/check loggin");
                return new StatusCodeResult(500);
            }
            try
            {
                if (loged_in)
                {
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
                            while (reader.Read())
                            {
                                cocktail.ID = reader["ID"].ToString();
                                cocktail.name = reader["name"].ToString();
                                cocktail.code = reader["code"].ToString();
                            }
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
                log.LogError(ex + "        --------> MakeCocktail/get cocktail");
                return new StatusCodeResult(500);
            }
            try
            {
                if(!String.IsNullOrEmpty(trial.machine_id))
                {
                    RegistryManager manager = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IOTHubMainConnectionstring"));
                    var twin = await manager.GetTwinAsync("testpi");
                    if(twin.ConnectionState.ToString() == "Connected")
                    {
                        if(twin.Properties.Desired["state"] == "awake")
                        {
                            twin.Properties.Desired["state"] = "busy";
                            await manager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);

                            ServiceClient sCli;
                            string iotconstr = Environment.GetEnvironmentVariable("IOTHubMainConnectionstring");
                            sCli = ServiceClient.CreateFromConnectionString(iotconstr);
                            CloudToDeviceMethod method = new CloudToDeviceMethod("make");
                            Dictionary<string, string> msg = new Dictionary<string, string>
                            {
                                { "code", cocktail.code }
                            };
                            string data = JsonConvert.SerializeObject(msg, Formatting.Indented);
                            method.SetPayloadJson(data);
                            CloudToDeviceMethodResult r = await sCli.InvokeDeviceMethodAsync("testpi", method);
                            log.LogInformation(r.GetPayloadAsJson());

                            RegistryManager man = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IOTHubMainConnectionstring"));
                            var t = await man.GetTwinAsync("testpi");
                            t.Properties.Desired["state"] = "awake";
                            await manager.UpdateTwinAsync(t.DeviceId, t, t.ETag);
                            return new OkObjectResult("{\"result\":\"succes\"}");
                        }
                        else
                        {
                            return new OkObjectResult("{\"result\":\"machine is busy\"}");
                        }
                    }
                    else
                    {
                        return new OkObjectResult("{\"result\":\"machine is not connected\"}");
                    }
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
