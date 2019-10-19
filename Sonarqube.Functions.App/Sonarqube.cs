using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sonarqube.Functions.App
{
    public static class Sonarqube
    {
        public static async Task<IEnumerable<string>> PluginsInstalled(string url, string token)
        {
            return await Get($"{url}/api/plugins/installed", token);
        }

        public static async Task<IEnumerable<string>> PluginsUpdate(string url, string token)
        {
            return await Get($"{url}/api/plugins/update", token);
        }

        public static async Task<IEnumerable<string>> Get(string url, string token)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:"));
            request.Headers.Add("Authorization", $"Basic {base64}");

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

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
            await Action($"{url}/api/plugins/install", token, key);
        }

        public static async Task UpdatePlugins(string url, string token, IEnumerable<string> plugins)
        {
            var tasks = new List<Task>();
            foreach (var plugin in plugins)
            {
                tasks.Add(UpdatePlugin(url, token, plugin));
            }
            await Task.WhenAll(tasks.ToArray());
        }

        public static async Task UpdatePlugin(string url, string token, string key)
        {
            await Action($"{url}/api/plugins/update", token, key);
        }

        public static async Task Action(string url, string token, string key)
        {

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post,
            };

            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}:"));
            request.Headers.Add("Authorization", $"Basic {base64}");

            request.Content = new StringContent($"key={key}", Encoding.UTF8, "application/x-www-form-urlencoded");

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();
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

                response.EnsureSuccessStatusCode();
            }
        }
    }
}
