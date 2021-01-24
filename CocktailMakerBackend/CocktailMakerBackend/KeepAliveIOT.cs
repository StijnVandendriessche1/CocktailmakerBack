using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices;
using System.Text;

namespace CocktailMakerBackend
{
    public static class KeepAliveIOT
    {
        [FunctionName("KeepAliveIOT")]
        public static async void Run([TimerTrigger("0/13 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                RegistryManager manager = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IOTHubMainConnectionstring"));
                var twin = await manager.GetTwinAsync("testpi");
                if (twin.ConnectionState.ToString() == "Connected")
                {
                    log.LogInformation("fired");
                    ServiceClient sCli;
                    string iotconstr = Environment.GetEnvironmentVariable("IOTHubMainConnectionstring");
                    sCli = ServiceClient.CreateFromConnectionString(iotconstr);
                    string device = "testpi";
                    var message = new Message(Encoding.ASCII.GetBytes("keep alive"));
                    await sCli.SendAsync(device, message);
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex + "        --------> keep alive");
            }
        }
    }
}
