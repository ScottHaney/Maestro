using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core
{
    public class InternalClassGraphWithEdgesRemoved : IInternalClassGraph
    {
        private readonly IInternalClassGraph _wrappedClassGraph;
        private readonly HashSet<InternalClassNodePair> _removedEdges;

        public InternalClassGraphWithEdgesRemoved(IInternalClassGraph wrappedClassGraph,
            HashSet<InternalClassNodePair> removedEdges)
        {
            _wrappedClassGraph = wrappedClassGraph;
            _removedEdges = removedEdges;
        }

        public IEnumerable<InternalClassNodePair> GetEdges()
        {
            return _wrappedClassGraph.GetEdges()
                .Where(x => !_removedEdges.Contains(x));
        }

        public IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node)
        {
            foreach (var neighbor in _wrappedClassGraph.GetNeighbors(node))
            {
                var edge = new InternalClassNodePair(neighbor, node);
                if (!_removedEdges.Contains(edge))
                    yield return neighbor;
            }
        }
    }
}
