using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maestro.Core
{
    public class BigClassHelper
    {
        public InternalClassDiagram CreateDiagram(string csFileWithClass)
        {
            var tree = CSharpSyntaxTree.ParseText(csFileWithClass);
            var root = tree.GetCompilationUnitRoot();

            var variables = GetVariableNodes(root).ToList();
            var methods = GetFunctionNodes(root, variables).ToList();

            return new InternalClassDiagram(variables, methods);
        }

        private IEnumerable<VariableNode> GetVariableNodes(CompilationUnitSyntax root)
        {
            var classNode = root.ChildNodes().OfType<ClassDeclarationSyntax>().Single();
            var fields = classNode.ChildNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var variable = field.Declaration.Variables.Single();
                yield return new VariableNode(variable.Identifier.ValueText);
            }
        }

        private IEnumerable<FunctionNode> GetFunctionNodes(CompilationUnitSyntax root, IEnumerable<VariableNode> variables)
        {
            var classNode = root.ChildNodes().OfType<ClassDeclarationSyntax>().Single();
            var methods = classNode.ChildNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var references = GetReferencedVariables(method, variables).ToList();
                yield return new FunctionNode(method.Identifier.ValueText, references);
            }
        }

        private IEnumerable<VariableNode> GetReferencedVariables(MethodDeclarationSyntax method, IEnumerable<VariableNode> nodes)
        {
            var refs = method.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();

            var names = new HashSet<string>(refs.Select(x => x.Identifier.ValueText).Distinct());
            return nodes.Where(x => names.Contains(x.Name));
        }
    }

    public class InternalClassDiagram
    {
        public readonly List<VariableNode> VariableNodes;
        public readonly List<FunctionNode> FunctionNodes;

        public bool IsEmpty => !VariableNodes.Any() && !FunctionNodes.Any();

        public InternalClassDiagram(List<VariableNode> variableNodes,
            List<FunctionNode> functionNodes)
        {
            VariableNodes = variableNodes;
            FunctionNodes = functionNodes;
        }
    }

    public class VariableNode
    {
        public readonly string Name;
        
        public VariableNode(string name)
        {
            Name = name;
        }
    }

    public class FunctionNode
    {
        public readonly string Name;
        public readonly List<VariableNode> References;

        public FunctionNode(string name,
            List<VariableNode> references)
        {
            Name = name;
            References = references;
        }
    }
}
