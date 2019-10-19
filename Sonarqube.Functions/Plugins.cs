using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sonarqube.Functions
{
    public static class Plugins
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [FunctionName("backup")]
        public static async Task<IActionResult> Backup(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("sonarqube/plugins", FileAccess.Write, Connection = "StorageConnection")] Stream stream,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

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


        [FunctionName("clean")]
        public static async Task<IActionResult> Restore(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = Environment.GetEnvironmentVariable("SonarqubeToken");

            var plugins = await Sonarqube.GetPlugins(url, token);
            return new OkObjectResult(null);
        }

        [FunctionName("restore")]
        public static async Task<IActionResult> Restore(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Blob("sonarqube/plugins", FileAccess.Read, Connection = "StorageConnection")] Stream stream,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var url = Environment.GetEnvironmentVariable("SonarqubeUrl");
            var token = Environment.GetEnvironmentVariable("SonarqubeToken");

            var plugins = await Sonarqube.GetPlugins(url, token);
            var content = JsonConvert.SerializeObject(plugins);
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);

            return new OkObjectResult(content);
        }
    }

    public static class Sonarqube
    {
        public static async Task<IEnumerable<string>> GetPlugins(string url, string token)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}/api/plugins/installed"),
                Method = HttpMethod.Get,
            };

            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:"));
            request.Headers.Add("Authorization", $"Basic {base64}");

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Status code: {response.StatusCode}, Content: {content}");
                }

                return JObject.Parse(content)
                    .Value<JArray>("plugins")
                    .Select(i => i.Value<string>("key"))
                    .ToArray();
            }
        }

        public static async Task UninstallPlugins(string url, string token, params string[] keys)
        {

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}/api/plugins/uninstall?key={keys}"),
                Method = HttpMethod.Get,
            };

            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:"));
            request.Headers.Add("Authorization", $"Basic {base64}");

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Status code: {response.StatusCode}, Content: {content}");
                }
            }
        }

        public static async Task UninstallPlugin(string url, string token, string key)
        {

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}/api/plugins/uninstall?key={key}"),
                Method = HttpMethod.Get,
            };

            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:"));
            request.Headers.Add("Authorization", $"Basic {base64}");

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Status code: {response.StatusCode}, Content: {content}");
                }
            }
        }
    }
}
