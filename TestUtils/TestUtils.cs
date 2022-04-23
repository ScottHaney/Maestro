using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Maestro.Core.Components;

namespace TestUtils
{
    public class TestWorkspaceBuilder
    {
        private readonly AdhocWorkspace _workspace = new AdhocWorkspace();

        public TestWorkspaceBuilder AddProject(string projectName)
        {
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp);
            var newProject = _workspace.AddProject(projectInfo);

            return this;
        }

        public TestWorkspaceBuilder AddDocument(string projectName, string documentName, string documentText)
        {
            var project = _workspace.CurrentSolution.Projects.First(x => x.Name == projectName);
            _workspace.AddDocument(project.Id, documentName, SourceText.From(documentText));

            return this;
        }

        public Workspace Build()
            => _workspace;
    }

    public static class TestHelpers
    {
        public static Workspace CreateSingleDocumentWorkspace(string testFileRelativePath)
        {
            var workspace = new AdhocWorkspace();

            string projName = "NewProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp);
            var newProject = workspace.AddProject(projectInfo);
            var sourceText = SourceText.From(TestHelpers.GetTestFileText(testFileRelativePath));
            workspace.AddDocument(newProject.Id, "NewFile.cs", sourceText);

            return workspace;
        }

        public static string GetTestFileText(string relativePath)
        {
            var filePath = Path.Combine(GetTestFilesBasePath(), relativePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            return File.ReadAllText(filePath);
        }

        private static string GetTestFilesBasePath()
        {
            var prefixToRemove = @"file:\";

            var result = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (result.StartsWith(prefixToRemove, StringComparison.OrdinalIgnoreCase))
                result = result.Substring(prefixToRemove.Length);

            return result;
        }
    }

    public class SingleTestDocumentWorkspace
    {
        private readonly Workspace _workspace;

        private SingleTestDocumentWorkspace(Workspace workspace)
        {
            _workspace = workspace;
        }

        public Workspace GetWorkspace()
            => _workspace;

        public SyntaxTree GetSyntaxTree()
        {
            return GetDocument().GetSyntaxTreeAsync().Result;
        }

        public string GetText()
        {
            return GetDocument().GetTextAsync().Result.ToString();
        }

        public WorkspaceComponentRegistry GetRegistry()
        {
            return new WorkspaceComponentRegistry(_workspace);
        }

        public bool TryUpdateSourceCode(string updatedSourceCode)
        {
            var updatedSolution = _workspace.CurrentSolution.WithDocumentText(GetDocument().Id, SourceText.From(updatedSourceCode));
            return _workspace.TryApplyChanges(updatedSolution);
        }

        public SemanticModel GetSemanticModel(SyntaxTree syntaxTree = null)
        {
            var compilation = GetProject().GetCompilationAsync().Result;
            var c = compilation.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location));

            var originalSyntaxTree = syntaxTree ?? GetDocument().GetSyntaxTreeAsync().Result;
            return c.GetSemanticModel(originalSyntaxTree);
        }

        public static SingleTestDocumentWorkspace Create(string testFileRelativePath)
        {
            return new SingleTestDocumentWorkspace(TestHelpers.CreateSingleDocumentWorkspace(testFileRelativePath));
        }

        private Project GetProject()
        {
            return _workspace.CurrentSolution.Projects.Single();
        }

        private Document GetDocument()
        {
            return GetProject().Documents.Single();
        }
    }
}
