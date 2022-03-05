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
        private readonly Dictionary<Node, List<Node>> _adjacencyList = new Dictionary<Node, List<Node>>();

        public void AddNode(VariableNode node)
        {
            _variables.Add(node);
        }

        public void AddNode(MethodNode node,
            List<VariableNode> variablesUsed,
            List<MethodNode> methodsCalled)
        {
            _adjacencyList[node] = variablesUsed.Concat<Node>(methodsCalled).ToList();
        }

        public IInternalClassGraph Build()
        {
            return new InternalClassGraph(_adjacencyList);
        }
    }
}
