using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core
{
    /// <summary>
    /// Finds properties of an <see cref="InternalClassGraph"/>
    /// </summary>
    public class InternalClassGraphAnalyzer
    {
        public ConnectedComponents FindConnectedComponents(BaseInternalClassGraph graph)
        {
            var result = new List<GraphComponent>();

            var allVisitedNodes = new HashSet<InternalClassNode>();
            while (allVisitedNodes.Count < graph.Nodes.Count)
            {
                var nextVisitedSet = new HashSet<InternalClassNode>();
                DFS(graph, graph.Nodes.First(x => !allVisitedNodes.Contains(x)), nextVisitedSet);

                result.Add(new GraphComponent(nextVisitedSet));
                allVisitedNodes.UnionWith(nextVisitedSet);
            }

            return new ConnectedComponents(result);
        }

        private void DFS(BaseInternalClassGraph graph, InternalClassNode startNode, HashSet<InternalClassNode> visitedNodes)
        {
            visitedNodes.Add(startNode);
            foreach (var node in graph.GetNeighbors(startNode))
            {
                if (!visitedNodes.Contains(node))
                    DFS(graph, node, visitedNodes);
            }
        }
    }

    public class ConnectedComponents
    {
        private readonly List<GraphComponent> _components;
        public IEnumerable<GraphComponent> Items => _components;

        public int Count => _components.Count;

        public ConnectedComponents(List<GraphComponent> components)
        {
            _components = components;
        }
    }

    public class GraphComponent : IEquatable<GraphComponent>
    {
        public readonly HashSet<InternalClassNode> Nodes;

        public GraphComponent(HashSet<InternalClassNode> nodes)
        {
            Nodes = nodes;
        }

        public static bool operator==(GraphComponent lhs, GraphComponent rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator!=(GraphComponent lhs, GraphComponent rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(GraphComponent other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Nodes.Count == other.Nodes.Count
                && Nodes.All(x => other.Nodes.Contains(x));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GraphComponent);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Nodes.Count == 0
                    ? 0
                    : Nodes.Sum(x => x.GetHashCode());
            }
        }
    }
}
