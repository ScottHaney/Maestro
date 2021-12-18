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

            return new InternalClassDiagram(GetVariableNodes(root).ToList());
        }

        private IEnumerable<VariableNode> GetVariableNodes(CompilationUnitSyntax root)
        {
            return Enumerable.Empty<VariableNode>();
        }
    }

    public class InternalClassDiagram
    {
        public readonly List<VariableNode> VariableNodes;

        public bool IsEmpty => !VariableNodes.Any();

        public InternalClassDiagram(List<VariableNode> variableNodes)
        {
            VariableNodes = variableNodes;
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
}
