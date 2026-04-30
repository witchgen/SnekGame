
namespace SnakeGame.Models.Github
{
    public class ReleaseInfo
    {
        public bool IsSuccesfulFetch { get; set; }
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string Changelog { get; set; }
    }
}
