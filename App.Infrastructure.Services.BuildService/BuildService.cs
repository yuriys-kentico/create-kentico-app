using System;
using System.Threading.Tasks;

using App.Core.Services;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace App.Infrastructure.Services
{
    public class BuildService : IBuildService
    {
        public BuildService()
        {
            MSBuildLocator.RegisterDefaults();
        }

        public async Task RebuildSolution(string solutionPath)
        {
            var workspace = MSBuildWorkspace.Create();

            var solution = await workspace.OpenSolutionAsync(solutionPath);

            foreach (var projectId in solution.GetProjectDependencyGraph().GetTopologicallySortedProjects())
            {
                var project = solution.GetProject(projectId);

                if (project == null)
                {
                    continue;
                }

                var projectCompilation = await project.GetCompilationAsync();

                if (projectCompilation == null)
                {
                    continue;
                }

                var result = projectCompilation.Emit(project.OutputFilePath);

                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.WriteLine(diagnostic.ToString());
                    }
                }
            }
        }
    }
}