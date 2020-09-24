using System;
using System.Reflection;

using App.Core.Context;
using App.Core.Models;

namespace App.Infrastructure.Context
{
    public class AppContext : IAppContext
    {
        public Terms Terms { get; }

        public Settings Settings { get; }

        public string Version { get; }

        public bool Source { get; set; }

        public bool Template { get; set; }

        public bool Mvc { get; set; }

        public bool Boilerplate { get; set; }

        public AppContext(
            Terms terms,
            Settings settings
            )
        {
            Terms = terms;
            Settings = settings;

            Version = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?? throw new ArgumentNullException("App version not found.");
        }
    }
}