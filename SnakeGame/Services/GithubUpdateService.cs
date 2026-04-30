
using Microsoft.Maui.ApplicationModel;
using SnakeGame.Models.Github;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SnakeGame.Services
{
    public interface IGithubUpdateService
    {
        Task<ReleaseInfo> CheckForAppUpdates();
    }

    class GithubUpdateService : IGithubUpdateService
    {
        private static string _projectUrl = "https://api.github.com/repos/witchgen/SnekGame/releases/latest";

        public async Task<ReleaseInfo> CheckForAppUpdates()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SnakeGame");

            var response = await client.GetAsync(_projectUrl);
            if(!response.IsSuccessStatusCode)
            {
                return new ReleaseInfo { IsSuccesfulFetch = false };
            }

            var stringJson = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<GithubRelease>(stringJson);

            return new ReleaseInfo
            {
                IsSuccesfulFetch = true,
                Version = info.TagName?.TrimStart('v'),
                DownloadUrl = info.Assets
                                .FirstOrDefault(d => d.AssetName.EndsWith(".apk")).DownloadUrl,
                Changelog = info.Body
            };
        }
    }
}
