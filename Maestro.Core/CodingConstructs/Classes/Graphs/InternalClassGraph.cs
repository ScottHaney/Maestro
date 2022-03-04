using Maestro.Core.CodingConstructs.Classes.Architecture;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public class InternalClassGraph : IInternalClassGraph
    {
        private readonly ICollection<Node> _nodes;

        public InternalClassGraph(ICollection<Node> nodes)
        {
            _nodes = nodes;
        }

        public IEnumerable<Node> GetNodes()
        {
            return _nodes;
        }
    }
}
