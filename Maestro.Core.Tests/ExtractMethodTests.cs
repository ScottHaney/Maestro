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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Maestro.Core.Tests
{
    public class ExtractMethodTests
    {
        [Test]
        public void Extract_Method_With_No_Return_Value_And_No_Arguments()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var workspace = SingleTestDocumentWorkspace.Create("TestCsFiles/ExtractMethodTestFile.cs");

                var updatedRoot = Helpers.ReplaceCodeWithMethod(workspace.GetSyntaxTree(), 10, 11, "TestMethod");

                if (!workspace.TryUpdateSourceCode(updatedRoot.GetText().ToString()))
                    Assert.Fail("Failed to update the source code");

                var textInUpdatedSolution = workspace.GetText();

                Assert.IsFalse(textInUpdatedSolution.Contains("var i"));
                Assert.IsFalse(textInUpdatedSolution.Contains("var j"));

                Assert.IsTrue(textInUpdatedSolution.Contains("TestMethod()"));
            }
        }
    }

    public class SingleTestDocumentWorkspace
    {
        private readonly Workspace _workspace;

        private SingleTestDocumentWorkspace(Workspace workspace)
        {
            _workspace = workspace;
        }

        public SyntaxTree GetSyntaxTree()
        {
            return GetDocument().GetSyntaxTreeAsync().Result;
        }

        public string GetText()
        {
            return GetDocument().GetTextAsync().Result.ToString();
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
            return new SingleTestDocumentWorkspace(TestUtils.CreateSingleDocumentWorkspace(testFileRelativePath));
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

    public static class Helpers
    {
        public static SyntaxNode ReplaceCodeWithMethod(SyntaxTree originalTree, int startLine, int endLine, string methodName)
        {
            var sourceText = originalTree.GetText();
            var root = originalTree.GetRoot();

            var lineMappings = sourceText.Lines;

            var nodesToRemove = new List<SyntaxNode>();
            for (int i = startLine; i <= endLine; i++)
            {
                nodesToRemove.Add(root.FindNode(lineMappings[i].Span));
            }

            var methodCall = CreateMethodCall(methodName);

            var parent = nodesToRemove.First().Parent;

            var isFirstCall = true;
            return root.ReplaceNodes(nodesToRemove, (original, secondArg) =>
            {
                if (isFirstCall)
                {
                    isFirstCall = false;
                    return methodCall;
                }
                else
                    return null;
            });
        }

        private static SyntaxNode CreateMethodCall(string methodName)
        {
            return ExpressionStatement(
                        InvocationExpression(
                            IdentifierName(methodName)));
        }
    }
}
