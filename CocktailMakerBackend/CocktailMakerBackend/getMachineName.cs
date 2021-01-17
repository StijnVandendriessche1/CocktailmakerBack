using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CocktailMakerBackend
{
    public static class getMachineName
    {
        [FunctionName("getMachineName")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "machine/{id}/name")] HttpRequest req, int id,
            ILogger log)
        {
            if(id == 0)
            {
                return new OkObjectResult("alfa 1.0");
            }
            else
            {
                return new BadRequestObjectResult("no machine with this id");
            }
        }
    }
}
