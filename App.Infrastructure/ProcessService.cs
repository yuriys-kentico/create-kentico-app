using System;
using System.Diagnostics;

using App.Core.Services;

namespace App.Infrastructure
{
    public class ProcessService : IProcessService
    {
        private Process? process;

        private Process Process
        {
            get => process ?? throw new InvalidOperationException($"Call '{nameof(NewProcess)}' first.");
            set => process = value;
        }

        public IProcessService NewProcess(string processPath)
        {
            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processPath,
                    UseShellExecute = false
                }
            };

            return this;
        }

        public IProcessService InDirectory(string? directoryPath)
        {
            Process.StartInfo.WorkingDirectory = directoryPath;
            return this;
        }

        public IProcessService WithArguments(string arguments)
        {
            Process.StartInfo.Arguments = arguments;
            return this;
        }

        public Process Run()
        {
            Process.Start();
            Process.WaitForExit();

            return Process;
        }
    }
}