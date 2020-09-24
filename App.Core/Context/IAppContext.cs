using App.Core.Models;

namespace App.Core.Context
{
    public interface IAppContext
    {
        public Terms Terms { get; }

        public Settings Settings { get; }

        public string Version { get; }

        bool Source { get; set; }

        bool Template { get; set; }

        bool Mvc { get; set; }

        bool Boilerplate { get; set; }
    }
}