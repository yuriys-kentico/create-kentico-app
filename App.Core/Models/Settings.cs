using System;

namespace App.Core.Models
{
    public class Settings
    {
        [Aliases("-n")]
        public string? AppName { get; set; }

        [Aliases("-p")]
        public string? AppPath { get; set; }

        [Aliases("-w")]
        public string? AppWebPath { get; set; }

        [Aliases("-v")]
        public Version? Version { get; set; }

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