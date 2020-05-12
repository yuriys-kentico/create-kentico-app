﻿using System;

using Microsoft.Extensions.DependencyInjection;

namespace App.Core.Tasks
{
    public class Tasks
    {
        private readonly IServiceProvider serviceProvider;

        public Tasks(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;
        }

        public IInstallTask InstallTask() => serviceProvider.GetRequiredService<IInstallTask>();

        public IHotfixTask HotfixTask() => serviceProvider.GetRequiredService<IHotfixTask>();

        public IIisTask IisTask() => serviceProvider.GetRequiredService<IIisTask>();

        public IDatabaseTask DatabaseTask() => serviceProvider.GetRequiredService<IDatabaseTask>();
    }
}