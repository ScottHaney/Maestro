using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Maestro.Core.Tests
{
    public static class TestUtils
    {
        public static Workspace CreateSingleDocumentWorkspace(string testFileRelativePath)
        {
            var workspace = new AdhocWorkspace();

            string projName = "NewProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp);
            var newProject = workspace.AddProject(projectInfo);
            var sourceText = SourceText.From(TestUtils.GetTestFileText(testFileRelativePath));
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
}
