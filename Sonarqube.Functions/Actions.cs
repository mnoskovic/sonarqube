using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sonarqube.Functions
{
    public static class Actions
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [FunctionName("clean-plugins")]
        public static async Task<IActionResult> Clean(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Sonarqube server - plugins clean");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = Environment.GetEnvironmentVariable("SonarqubeToken");

            var plugins = await Sonarqube.GetPlugins(url, token);
            await Sonarqube.UninstallPlugins(url, token, plugins);
            await Sonarqube.Restart(url, token);

            return new OkObjectResult(null);
        }


        [FunctionName("backup-plugins")]
        public static async Task<IActionResult> Backup(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("sonarqube/plugins", FileAccess.Write, Connection = "StorageConnection")] Stream stream,
            ILogger log)
        {
            log.LogInformation("Sonarqube server - plugins backup");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = Environment.GetEnvironmentVariable("SonarqubeToken");
            var plugins = await Sonarqube.GetPlugins(url, token);
            var content = JsonConvert.SerializeObject(plugins);

            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(content);
            }
            return new OkObjectResult(content);
        }



        [FunctionName("restore-plugins")]
        public static async Task<IActionResult> Restore(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("sonarqube/plugins", FileAccess.Read, Connection = "StorageConnection")] Stream stream,
            ILogger log)
        {
            log.LogInformation("Sonarqube server - plugins restore");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = Environment.GetEnvironmentVariable("SonarqubeToken");

            var existingPlugins = await Sonarqube.GetPlugins(url, token);

            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                var requiredPlugins = (IEnumerable<string>)JsonConvert.DeserializeObject(content);
                var missingPlugins = requiredPlugins.Except(existingPlugins);
                if (missingPlugins.Any())
                {

                    log.LogInformation($"Restoring sonarqube plugins: {String.Join(, missingPlugins)}");

                    await Sonarqube.InstallPlugins(url, token, missingPlugins);
                    await Sonarqube.Restart(url, token);
                }
            }

            return new OkObjectResult(null);
        }

        [FunctionName("restart-server")]
        public static async Task<IActionResult> Restart(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Sonarqube server - restarting");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = Environment.GetEnvironmentVariable("SonarqubeToken");

            await Sonarqube.Restart(url, token);

            return new OkObjectResult(null);
        }
    }
}
