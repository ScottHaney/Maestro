using Maestro.Core.CodingConstructs.Classes.Architecture;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public class RoslynCSharpClassParser : ICSharpClassParser
    {
        private readonly SyntaxNode _classDeclaration;

        public RoslynCSharpClassParser(SyntaxNode classDeclaration)
        {
            _classDeclaration = classDeclaration;
        }

        public IEnumerable<string> GetVariableNames()
        {
            var fields = _classDeclaration.ChildNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var variable = field.Declaration.Variables.Single();
                yield return variable.Identifier.ValueText;
            }
        }

        public IEnumerable<MethodReferences> GetMethodsInfo()
        {
            var methods = _classDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var methodNode = new MethodNode(method.Identifier.ValueText);
                var refs = GetNodesReferencedBy(method);

                yield return new MethodReferences(method.Identifier.ValueText, refs.VariablesUsed, refs.MethodsCalled);
            }
        }

        private (List<string> VariablesUsed, List<string> MethodsCalled) GetNodesReferencedBy(MethodDeclarationSyntax method)
        {
            var variablesUsed = new List<string>();
            var methodsCalled = new List<string>();

            foreach (var descendant in method.DescendantNodes())
            {
                if (descendant is IdentifierNameSyntax variableSyntax)
                    variablesUsed.Add(variableSyntax.Identifier.ValueText);
                else if (descendant is InvocationExpressionSyntax invocationSyntax)
                {
                    throw new NotImplementedException("Need to add code here to figure out the method name");
                    //methodsCalled.Add(new MethodNode());
                }
            }

            return (variablesUsed, methodsCalled);
        }
    }
}
