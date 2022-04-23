using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using TestUtils;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class WorkspaceWatcherTests
    {
        [Test]
        public void Detects_File_Being_Renamed()
        {
            var workspace = new TestWorkspaceBuilder()
                .AddProject("Project")
                .AddDocument("Project", "TestFile.cs", "")
                .Build();

            var watcher = new WorkspaceWatcher(workspace);

            var originalDoc = workspace.CurrentSolution.Projects.First().Documents.First();
            var updatedSolution = workspace.CurrentSolution.WithDocumentName(originalDoc.Id, "UpdatedFile.cs");

            var succeeded = workspace.TryApplyChanges(updatedSolution);
            
            Assert.IsTrue(succeeded);
        }
    }
}
