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
    public class InternalClassGraphGenerator : IInternalClassGraphGenerator
    {
        private readonly IInternalClassGraphBuilder _builder;

        public InternalClassGraphGenerator(IInternalClassGraphBuilder builder)
        {
            _builder = builder;
        }

        public IInternalClassGraph CreateGraph(SyntaxNode node, bool isDirectedGraph)
        {
            foreach (var variableNode in GetVariableNodes(node))
                _builder.AddNode(variableNode);

            foreach (var methodNode in GetMethodNodes(node))
                _builder.AddNode(methodNode.Node, methodNode.ReferencedVariables, methodNode.CalledMethods);

            return _builder.Build();
        }

        public IInternalClassGraph CreateGraph(string csFileWithClass, bool isDirectedGraph)
        {
            var tree = CSharpSyntaxTree.ParseText(csFileWithClass);
            var root = tree.GetCompilationUnitRoot();

            return CreateGraph(root, isDirectedGraph);
        }

        private IEnumerable<VariableNode> GetVariableNodes(SyntaxNode node)
        {
            var classNode = GetClassDeclaration(node);
            if (classNode == null)
                yield break;

            var fields = classNode.ChildNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var variable = field.Declaration.Variables.Single();
                yield return new VariableNode(variable.Identifier.ValueText);
            }
        }

        private IEnumerable<FunctionNodeResult> GetMethodNodes(SyntaxNode node)
        {
            var classNode = GetClassDeclaration(node);
            if (classNode == null)
                yield break;

            var methods = classNode.ChildNodes().OfType<MethodDeclarationSyntax>();

            var result = new Dictionary<InternalClassNode, List<InternalClassNode>>();
            foreach (var method in methods)
            {
                var methodNode = new MethodNode(method.Identifier.ValueText);
                var refs = GetNodesReferencedBy(method);

                yield return new FunctionNodeResult(methodNode, refs.VariablesUsed, refs.MethodsCalled);
            }
        }

        private ClassDeclarationSyntax GetClassDeclaration(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax classDecl)
                return classDecl;
            else
                return node.ChildNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }

        private (List<VariableNode> VariablesUsed, List<MethodNode> MethodsCalled) GetNodesReferencedBy(MethodDeclarationSyntax method)
        {
            var variablesUsed = new List<VariableNode>();
            var methodsCalled = new List<MethodNode>();

            foreach (var descendant in method.DescendantNodes())
            {
                if (descendant is IdentifierNameSyntax variableSyntax)
                    variablesUsed.Add(new VariableNode(variableSyntax.Identifier.ValueText));
                else if (descendant is InvocationExpressionSyntax invocationSyntax)
                {
                    throw new NotImplementedException("Need to add code here to figure out the method name");
                    //methodsCalled.Add(new MethodNode());
                }
            }

            return (variablesUsed, methodsCalled);
        }

        public IInternalClassGraph Generate()
        {
            return _builder.Build();
        }

        private class FunctionNodeResult
        {
            public readonly MethodNode Node;
            public readonly List<VariableNode> ReferencedVariables;
            public readonly List<MethodNode> CalledMethods;

            public FunctionNodeResult(MethodNode node,
                List<VariableNode> referencedVariables,
                List<MethodNode> calledMethods)
            {
                Node = node;
                ReferencedVariables = referencedVariables;
                CalledMethods = calledMethods;
            }
        }
    }
}
