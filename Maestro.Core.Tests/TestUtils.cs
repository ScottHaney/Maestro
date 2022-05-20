using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class TestWorkspaceBuilder
    {
        private readonly AdhocWorkspace _workspace = new AdhocWorkspace();

        public TestWorkspaceBuilder(string solutionFilePath)
        {
            _workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(),
                VersionStamp.Create(),
                solutionFilePath));
        }

        public TestWorkspaceBuilder AddProject(string projectName)
        {
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp,
                Path.Combine(Path.GetDirectoryName(_workspace.CurrentSolution.FilePath), projectName));

            var newProject = _workspace.AddProject(projectInfo);

            return this;
        }

        public TestWorkspaceBuilder AddDocument(string projectName, string documentName, string documentText)
        {
            var project = _workspace.CurrentSolution.Projects.First(x => x.Name == projectName);
            var doc = _workspace.AddDocument(project.Id, documentName, SourceText.From(documentText));

            var documentFilePath = Path.Combine(Path.GetDirectoryName(_workspace.CurrentSolution.FilePath), documentName);
            _workspace.TryApplyChanges(_workspace.CurrentSolution.WithDocumentFilePath(doc.Id, documentFilePath));

            return this;
        }

        public Workspace Build()
            => _workspace;
    }
}