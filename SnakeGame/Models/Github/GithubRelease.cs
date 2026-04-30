using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SnakeGame.Models.Github
{
    public class GithubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }
        [JsonPropertyName("assets")]
        public List<ReleaseAsset> Assets { get; set; }

        public class ReleaseAsset
        {
            [JsonPropertyName("name")]
            public string AssetName { get; set; }
            [JsonPropertyName("browser_download_url")]
            public string DownloadUrl { get; set; }
        }
    }
}
