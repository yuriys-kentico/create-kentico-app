namespace App.Core.Models
{
    public class Settings
    {
        [Aliases("-h")]
        public bool? Help { get; set; }

        [Aliases("-n")]
        public string? Name { get; set; }

        [Aliases("-p")]
        public string? Path { get; set; }

        [Aliases("-ad")]
        public string? AdminDomain { get; set; }

        [Aliases("-d")]
        public string? AppDomain { get; set; }

        [Aliases("-t")]
        public string? AppTemplate { get; set; }

        [Aliases("-v")]
        public KenticoVersion? Version { get; set; }

        [Aliases("-s")]
        public bool? Source { get; set; }

        [Aliases("-sp")]
        public string? SourcePassword { get; set; }

        [Aliases("-db")]
        public string? DatabaseName { get; set; }

        [Aliases("-ds")]
        public string? DatabaseServerName { get; set; }

        [Aliases("-du")]
        public string? DatabaseServerUser { get; set; }

        [Aliases("-dp")]
        public string? DatabaseServerPassword { get; set; }

        [Aliases("-l")]
        public string? License { get; set; }
    }
}