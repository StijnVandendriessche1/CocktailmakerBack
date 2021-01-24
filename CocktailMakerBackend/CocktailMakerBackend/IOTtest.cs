using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices;
using System;

namespace CocktailMakerBackend
{
    public static class IOTtest
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("IOTtest")]
        public static async void Run([IoTHubTrigger("messages/events", Connection = "IOTHubConnectionstring")]EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
            if(Encoding.UTF8.GetString(message.Body.Array) == "awake")
            {
                RegistryManager manager = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IOTHubMainConnectionstring"));
                var twin = await manager.GetTwinAsync("testpi");
                twin.Properties.Desired["state"] = "awake";
                await manager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
            }
        }
    }
}