using System.Threading.Tasks;

namespace BuildService
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var buildService = new BuildService();

            await buildService.RebuildSolution(args[0]);
        }
    }
}