using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sonarqube.Functions
{
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

        public static async Task InstallPlugins(string url, string token, IEnumerable<string> plugins)
        {
            var tasks = new List<Task>();
            foreach (var plugin in plugins)
            {
                tasks.Add(InstallPlugin(url, token, plugin));
            }
            await Task.WhenAll(tasks.ToArray());
        }


        public static async Task InstallPlugin(string url, string token, string key)
        {

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}/api/plugins/install?key={key}"),
                Method = HttpMethod.Post,
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


        public static async Task UninstallPlugins(string url, string token, IEnumerable<string> plugins)
        {
            var tasks = new List<Task>();
            foreach (var plugin in plugins)
            {
                tasks.Add(UninstallPlugin(url, token, plugin));
            }
            await Task.WhenAll(tasks.ToArray());
        }

        public static async Task UninstallPlugin(string url, string token, string key)
        {

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}/api/plugins/uninstall?key={key}"),
                Method = HttpMethod.Post,
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

        public static async Task Restart(string url, string token)
        {

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{url}/api/system/restart"),
                Method = HttpMethod.Post,
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
