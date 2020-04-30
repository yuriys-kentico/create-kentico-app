using System;

namespace App.Core.Models
{
    public class Settings
    {
        private KenticoVersion? version;

        [Aliases("-n")]
        public string? AppName { get; set; }

        [Aliases("-p")]
        public string? AppPath { get; set; }

        [Aliases("-w")]
        public string? AppWebPath { get; set; }

        [Aliases("-v")]
        public string? Version { get; set; }

        public KenticoVersion? ParsedVersion
        {
            get => version ?? (Version != null ? new KenticoVersion(Version) : null);
            set => version = value;
        }

        [Aliases("-d", "-dn")]
        public string? DbName { get; set; }

        [Aliases("-s", "-sn")]
        public string? DbServerName { get; set; }

        [Aliases("-su")]
        public string? DbServerUser { get; set; }

        [Aliases("-sp")]
        public string? DbServerPassword { get; set; }
    }
}