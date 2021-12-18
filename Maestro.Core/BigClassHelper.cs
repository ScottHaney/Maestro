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

            return new InternalClassDiagram(GetVariableNodes(root).ToList(), GetFunctionNodes(root).ToList());
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

        private IEnumerable<FunctionNode> GetFunctionNodes(CompilationUnitSyntax root)
        {
            return Enumerable.Empty<FunctionNode>();
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

        public FunctionNode(string name)
        {
            Name = name;
        }
    }
}
