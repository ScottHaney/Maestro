using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class InternalClassGraphBuilder
    {
        private readonly List<InternalClassNode> _nodes;
        private readonly InternalClassNodeAdjacencyMatrixBuilder _adjacencyMatrixBuilder;

        public InternalClassGraphBuilder(List<InternalClassNode> nodes, bool isDirectedGraph)
        {
            _nodes = nodes;
            _adjacencyMatrixBuilder = new InternalClassNodeAdjacencyMatrixBuilder(isDirectedGraph);
        }

        public void AddAdjacency(InternalClassNode source, List<InternalClassNode> neighbors)
        {
            _adjacencyMatrixBuilder.AddNeighbors(source, neighbors);
        }

        public InternalClassGraph Build()
        {
            return new InternalClassGraph(_nodes, _adjacencyMatrixBuilder.Build());
        }
    }
}
