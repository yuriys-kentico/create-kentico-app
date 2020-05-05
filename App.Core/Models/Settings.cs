namespace App.Core.Models
{
    public class Settings
    {
        [Aliases("-n")]
        public string? Name { get; set; }

        [Aliases("-p")]
        public string? Path { get; set; }

        [Aliases("-aad")]
        public string? AdminDomain { get; set; }

        [Aliases("-ad")]
        public string? AppDomain { get; set; }

        [Aliases("-t", "-at")]
        public string? AppTemplate { get; set; }

        [Aliases("-v", "-av")]
        public KenticoVersion? Version { get; set; }

        [Aliases("-s")]
        public bool? Source { get; set; }

        [Aliases("-sp")]
        public string? SourcePassword { get; set; }

        [Aliases("-d", "-dn")]
        public string? DbName { get; set; }

        [Aliases("-ds")]
        public string? DbServerName { get; set; }

        [Aliases("-du")]
        public string? DbServerUser { get; set; }

        [Aliases("-dp")]
        public string? DbServerPassword { get; set; }
    }
}