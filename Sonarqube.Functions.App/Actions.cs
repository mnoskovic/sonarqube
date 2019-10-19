using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sonarqube.Functions.App
{
    public static class Actions
    {

        [FunctionName("install-plugins")]
        public static async Task<IActionResult> Restore(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Sonarqube server - plugins restore");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = req.Headers["sonarqube-token"].FirstOrDefault() ?? Environment.GetEnvironmentVariable("SonarqubeToken");

            var existingPlugins = await Sonarqube.PluginsInstalled(url, token);
            
            using (var reader = new StreamReader(req.Body))
            {
                var content = await reader.ReadToEndAsync();
                var requiredPlugins = JArray.Parse(content).Select(i => i.Value<string>());
                var missingPlugins = requiredPlugins.Except(existingPlugins);
                if (missingPlugins.Any())
                {
                    log.LogInformation($"Installing sonarqube plugins: {string.Join(",", missingPlugins)}");

                    await Sonarqube.InstallPlugins(url, token, missingPlugins);
                    await Sonarqube.Restart(url, token);
                }
            }

            return new OkObjectResult(null);
        }
        
        [FunctionName("update-plugins")]
        public static async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Sonarqube server - plugins clean");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = req.Headers["sonarqube-token"].FirstOrDefault() ?? Environment.GetEnvironmentVariable("SonarqubeToken");

            var plugins = await Sonarqube.PluginsUpdate(url, token);
            if (plugins.Any())
            {
                log.LogInformation($"Updating plugins: {string.Join(",", plugins)}");

                await Sonarqube.UpdatePlugins(url, token, plugins);
                await Sonarqube.Restart(url, token);
            }
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
            var token = req.Headers["sonarqube-token"].FirstOrDefault() ?? Environment.GetEnvironmentVariable("SonarqubeToken");

            var plugins = await Sonarqube.PluginsInstalled(url, token);
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
            var token = req.Headers["sonarqube-token"].FirstOrDefault() ?? Environment.GetEnvironmentVariable("SonarqubeToken");

            var existingPlugins = await Sonarqube.PluginsInstalled(url, token);

            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                var requiredPlugins = JArray.Parse(content).Select(i => i.Value<string>());
                var missingPlugins = requiredPlugins.Except(existingPlugins);
                if (missingPlugins.Any())
                {
                    log.LogInformation($"Restoring sonarqube plugins: {string.Join(",", missingPlugins)}");

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
            var token = req.Headers["sonarqube-token"].FirstOrDefault() ?? Environment.GetEnvironmentVariable("SonarqubeToken");

            await Sonarqube.Restart(url, token);

            return new OkObjectResult(null);
        }
    }
}
