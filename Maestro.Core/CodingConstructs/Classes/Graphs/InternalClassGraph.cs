using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public class InternalClassGraph : IInternalClassGraph
    {
        private readonly Dictionary<Node, List<Node>> _adjacencyList;

        public InternalClassGraph(Dictionary<Node, List<Node>> adjacencyList)
        {
            _adjacencyList = adjacencyList;
        }

        public IEnumerable<Node> GetNodes()
        {
            return _adjacencyList.Keys;
        }

        public IEnumerable<Node> GetNeighbors(Node node)
        {
            return _adjacencyList[node];
        }
    }
}
