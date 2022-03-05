using Maestro.Core.CodingConstructs.Classes.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public class InternalClassGraphBuilder : IInternalClassGraphBuilder
    {
        private readonly List<VariableNode> _variables = new List<VariableNode>();

        private readonly Dictionary<MethodNode, List<MethodNode>> _methodsCalled = new Dictionary<MethodNode, List<MethodNode>>();
        private readonly Dictionary<MethodNode, List<VariableNode>> _variablesUsed = new Dictionary<MethodNode, List<VariableNode>>();

        public void AddNode(VariableNode node)
        {
            _variables.Add(node);
        }

        public void AddNode(MethodNode node,
            List<VariableNode> variablesUsed,
            List<MethodNode> methodsCalled)
        {
            _methodsCalled[node] = methodsCalled;
            _variablesUsed[node] = variablesUsed;
        }

        public IInternalClassGraph Build()
        {
            var nodes = new List<Node>();
            nodes.AddRange(_variables);
            nodes.AddRange(_methodsCalled.Keys);

            return new InternalClassGraph(nodes);
        }
    }
}
