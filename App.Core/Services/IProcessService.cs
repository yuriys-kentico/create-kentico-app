using System.Diagnostics;

namespace App.Core.Services
{
    public interface IProcessService
    {
        public IProcessService FromPath(string processPath);

        public IProcessService InDirectory(string? directoryPath);

        public IProcessService WithArguments(string arguments);

        public Process Run();
    }
}