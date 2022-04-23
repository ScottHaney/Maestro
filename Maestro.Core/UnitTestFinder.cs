using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Maestro.Core
{
    public class UnitTestFinder
    {
        private Workspace _currentWorkspace;

        public UnitTestFinder(Workspace currentWorkspace)
        {
            _currentWorkspace = currentWorkspace;
        }

        public Document FindUnitTest(Document csharpFile)
        {
            var projectName = csharpFile.Project.Name;

            var currentSolution = csharpFile.Project.Solution;

            var expectedTestsProjectName = $"{projectName}.Tests";
            var testsProject = currentSolution.Projects.FirstOrDefault(x => string.Compare(x.Name, expectedTestsProjectName, StringComparison.OrdinalIgnoreCase) == 0);

            if (testsProject == null)
                return null;

            var expectedTestFileName = $"{Path.GetFileNameWithoutExtension(csharpFile.Name)}Tests.cs";
            var testFile = testsProject.Documents.FirstOrDefault(x => string.Compare(x.Name, expectedTestFileName, StringComparison.OrdinalIgnoreCase) == 0);

            return testFile;
        }
    }
}
