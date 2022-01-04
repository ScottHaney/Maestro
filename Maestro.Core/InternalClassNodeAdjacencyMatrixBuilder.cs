using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core
{
    public class InternalClassNodeAdjacencyMatrixBuilder
    {
        private Dictionary<InternalClassNode, HashSet<InternalClassNode>> _map = new Dictionary<InternalClassNode, HashSet<InternalClassNode>>();
        private readonly bool _isDirectedGraph;

        public InternalClassNodeAdjacencyMatrixBuilder(bool isDirectedGraph)
        {
            _isDirectedGraph = isDirectedGraph;
        }

        public void AddNeighbors(InternalClassNode source, List<InternalClassNode> neighbors)
        {
            AddNeighborsInternal(source, neighbors);

            if (!_isDirectedGraph)
            {
                foreach (var neighbor in neighbors)
                    AddNeighborsInternal(neighbor, new List<InternalClassNode>() { source });
            }
        }

        private void AddNeighborsInternal(InternalClassNode source, List<InternalClassNode> neighbors)
        {
            if (_map.TryGetValue(source, out var values))
            {
                foreach (var neighbor in neighbors)
                    values.Add(neighbor);
            }
            else if (neighbors.Any())
                _map[source] = new HashSet<InternalClassNode>(neighbors);
        }

        public InternalClassNodeAdjacencyMatrix Build()
        {
            return new InternalClassNodeAdjacencyMatrix(_map);
        }
    }
}
