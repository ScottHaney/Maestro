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
using Maestro.Core.Components;
using TestUtils;

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
