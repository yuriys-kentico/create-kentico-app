﻿using App.Core.Tasks;

namespace App.Core
{
    public interface IInstallTask : ITask
    {
        bool Source { get; set; }
    }
}