using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core
{
    public class ConnectedComponentsFinder
    {
        public List<GraphComponent> Find(InternalClassGraph graph)
        {
            var result = new List<GraphComponent>();

            var allVisitedNodes = new HashSet<InternalClassNode>();
            while (allVisitedNodes.Count < graph.Nodes.Count)
            {
                var nextVisitedSet = new HashSet<InternalClassNode>();
                DFS(graph, graph.Nodes.First(x => !allVisitedNodes.Contains(x)), nextVisitedSet);

                result.Add(new GraphComponent(nextVisitedSet.ToList()));
                allVisitedNodes.UnionWith(nextVisitedSet);
            }

            return result;
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
