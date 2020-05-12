using System.Threading.Tasks;

namespace BuildService
{
    public interface IBuildService
    {
        Task RebuildSolution(string solutionPath);
    }
}