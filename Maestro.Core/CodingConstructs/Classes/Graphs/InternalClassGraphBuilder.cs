using Maestro.Core.CodingConstructs.Classes.Architecture;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public class InternalClassGraphBuilder : IInternalClassGraphBuilder
    {
        private readonly List<Node> _nodes = new List<Node>();

        public void AddNode(VariableNode node)
        {
            throw new NotImplementedException();
        }

        public void AddNode(MethodNode node)
        {
            throw new NotImplementedException();
        }

        public IInternalClassGraph Build()
        {
            return new InternalClassGraph(_nodes);
        }
    }
}
