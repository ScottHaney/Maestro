using Autofac;
using Autofac.Extras.Moq;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Maestro.Core.Tests
{
    public class ExtractMethodTests
    {
        [Test]
        public void Test()
        {
            using (var mock = AutoMock.GetLoose(cb =>
            {

            }))
            {
                var workspace = new AdhocWorkspace();

                string projName = "NewProject";
                var projectId = ProjectId.CreateNewId();
                var versionStamp = VersionStamp.Create();
                var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp);
                var newProject = workspace.AddProject(projectInfo);
                var sourceText = SourceText.From(TestUtils.GetTestFileText("TestCsFiles/ExtractMethodTestFile.cs"));
                var newDocument = workspace.AddDocument(newProject.Id, "NewFile.cs", sourceText);

                var project = workspace.CurrentSolution.Projects.Single();
                var compilation = project.GetCompilationAsync().Result;

                var c = compilation.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location));

                var originalSyntaxTree = project.Documents.Single().GetSyntaxTreeAsync().Result;
                var semanticModel = c.GetSemanticModel(originalSyntaxTree);

                var testMethodDeclaration = semanticModel.SyntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .First(x => x.Identifier.Text == "TestMethod");

                var startLineToRemove = 10;
                var endLineToRemove = 11;

                var lineMappings = semanticModel.SyntaxTree.GetText().Lines;
                var startIndex = lineMappings[startLineToRemove].Span.Start;
                var endIndex = lineMappings[endLineToRemove].Span.End;

                var originalText = originalSyntaxTree.GetText().ToString();
                var updatedText = string.Concat(originalText.Substring(0, startIndex), originalText.Substring(endIndex + 1));
                
                var updatedSolution = workspace.CurrentSolution.WithDocumentText(project.Documents.Single().Id, SourceText.From(updatedText));

                var succeded = workspace.TryApplyChanges(updatedSolution);

                var textInUpdatedSolution = workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync().Result;


            }
        }
    }
}
