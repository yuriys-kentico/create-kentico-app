using System.Threading.Tasks;

namespace App.Core.Services
{
    public interface IBuildService
    {
        Task RebuildSolution(string solutionPath);
    }
}