using System;

namespace App.Core.Models
{
    public class KenticoVersion
    {
        private readonly Version version;

        public int Major => version.Major;

        public int Minor => version.Minor;

        public int Build => version.Build;

        public KenticoVersion(string version)
        {
            if (Version.TryParse(version, out var result))
            {
                this.version = result;
            }
            else if (Version.TryParse($"{version}.0", out result))
            {
                this.version = result;
            }
            else
            {
                throw new ArgumentException($"'{version}' is not a valid version or partial version.");
            }
        }

        public KenticoVersion(int major) : this(major.ToString())
        {
        }

        protected KenticoVersion(Version version) => this.version = version;

        public static implicit operator KenticoVersion(Version version) => new KenticoVersion(version);

        public static implicit operator Version(KenticoVersion? version) => version?.version ?? throw new ArgumentNullException(nameof(version));
    }
}