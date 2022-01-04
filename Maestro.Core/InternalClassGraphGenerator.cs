using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maestro.Core
{
    /// <summary>
    /// Diagrams the relationships between variables and functions inside of a class
    /// </summary>
    public class InternalClassGraphGenerator
    {
        public InternalClassGraph CreateGraph(SyntaxNode node, bool isDirectedGraph)
        {
            var variables = GetVariableNodes(node).ToList();
            var methodsMap = GetFunctionNodes(node, variables);

            var allNodes = variables.Concat(methodsMap.Keys).ToList();
            var builder = new InternalClassGraphBuilder(allNodes, isDirectedGraph);

            foreach (var entry in methodsMap)
                builder.AddAdjacency(entry.Key, entry.Value);

            return builder.Build();
        }

        public InternalClassGraph CreateGraph(string csFileWithClass, bool isDirectedGraph)
        {
            var tree = CSharpSyntaxTree.ParseText(csFileWithClass);
            var root = tree.GetCompilationUnitRoot();

            return CreateGraph(root, isDirectedGraph);
        }

        private IEnumerable<InternalClassNode> GetVariableNodes(SyntaxNode node)
        {
            var classNode = GetClassDeclaration(node);
            if (classNode == null)
                yield break;

            var fields = classNode.ChildNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var variable = field.Declaration.Variables.Single();
                yield return new InternalClassNode(variable.Identifier.ValueText, InternalClassNodeType.Variable);
            }
        }

        private Dictionary<InternalClassNode, List<InternalClassNode>> GetFunctionNodes(SyntaxNode node, List<InternalClassNode> variableNodes)
        {
            var classNode = GetClassDeclaration(node);
            if (classNode == null)
                return new Dictionary<InternalClassNode, List<InternalClassNode>>();

            var methods = classNode.ChildNodes().OfType<MethodDeclarationSyntax>();

            var result = new Dictionary<InternalClassNode, List<InternalClassNode>>();
            foreach (var method in methods)
            {
                var variableRefs = GetReferencedVariables(method, variableNodes);
                var methodNode = new InternalClassNode(method.Identifier.ValueText, InternalClassNodeType.Function);

                result[methodNode] = variableRefs.ToList();
            }

            return result;
        }

        private ClassDeclarationSyntax GetClassDeclaration(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax classDecl)
                return classDecl;
            else
                return node.ChildNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }

        private IEnumerable<InternalClassNode> GetReferencedVariables(MethodDeclarationSyntax method, IEnumerable<InternalClassNode> nodes)
        {
            var refs = method.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();

            var names = new HashSet<string>(refs.Select(x => x.Identifier.ValueText).Distinct());
            return nodes.Where(x => names.Contains(x.Name));
        }
    }
}
