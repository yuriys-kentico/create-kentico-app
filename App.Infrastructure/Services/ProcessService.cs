using System.Diagnostics;

using App.Core.Services;

namespace App.Infrastructure.Services
{
    public class ProcessService : IProcessService
    {
        private readonly IOutputService output;

        private Process Process { get; set; } = new Process();

        public ProcessService(ServiceResolver services)
        {
            output = services.OutputService();
        }

        public IProcessService FromPath(string processPath)
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
            Process.StartInfo.RedirectStandardOutput = true;
            Process.OutputDataReceived += (sender, e) => output.Display(e.Data);

            Process.Start();
            Process.BeginOutputReadLine();
            Process.WaitForExit();

            var oldProcess = Process;

            Process = new Process();

            return oldProcess;
        }
    }
}