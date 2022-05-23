using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Maestro.Core.Links
{
    public class WhichItemsShouldBeLinked
    {
        private readonly Workspace _workspace;

        public WhichItemsShouldBeLinked(Workspace workspace)
        {
            _workspace = workspace;
        }

        public IEnumerable<ProjectItem> GetLinks(ProjectItem projectItem)
        {
            var testClassFinder = new TestClassFinder(_workspace);

            var testClass = testClassFinder.GetTestClass(projectItem);
            if (testClass != null)
                yield return testClass;
        }
    }

    public class TestClassFinder
    {
        private readonly Workspace _workspace;

        public TestClassFinder(Workspace workspace)
        {
            _workspace = workspace;
        }

        public ProjectItem GetTestClass(ProjectItem projectItem)
        {
            var projectFileName = projectItem.GetFullProjectPath(_workspace.CurrentSolution.FilePath);
            if (!string.IsNullOrEmpty(projectFileName))
            {
                var projectName = Path.GetFileNameWithoutExtension(projectFileName);
                foreach (var project in _workspace.CurrentSolution.Projects)
                {
                    var currentProjectName = Path.GetFileNameWithoutExtension(project.FilePath);
                    if (string.Compare(projectName + ".Tests", currentProjectName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var itemName = Path.GetFileNameWithoutExtension(projectItem.FileName);
                        var itemExt = Path.GetExtension(projectItem.FileName);

                        var targetName = itemName + "Tests" + itemExt;
                        foreach (var document in project.Documents)
                        {
                            if (string.Compare(Path.GetFileName(document.FilePath), targetName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                var solutionDirectory = Path.GetDirectoryName(_workspace.CurrentSolution.FilePath);
                                return new ProjectItem(PathNetCore.GetRelativePath(solutionDirectory, project.FilePath), PathNetCore.GetRelativePath(solutionDirectory, document.FilePath));
                            }
                        }
                    }
                }

            }

            return null;
        }
    }
}
