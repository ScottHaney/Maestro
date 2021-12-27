using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class ConnectedComponentsFinder
    {
        public List<GraphComponent> Find(InternalClassGraph graph)
        {
            throw new NotImplementedException();
        }

        private void DFS(InternalClassGraph graph, InternalClassNode startNode, HashSet<InternalClassNode> visitedNodes)
        {
            visitedNodes.Add(startNode);
            foreach (var node in graph.GetNeighbors(startNode))
            {
                if (!visitedNodes.Contains(node))
                    DFS(graph, node, visitedNodes);
            }
        }
    }

    public class GraphComponent
    {
        public readonly List<InternalClassNode> Nodes;

        public GraphComponent(List<InternalClassNode> nodes)
        {
            Nodes = nodes;
        }
    }
}
