using Maestro.Core.CodingConstructs.Classes.Graphs;
using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes
{
    public class InternalClassGraphAnalyzer : IInternalClassGraphAnalyzer
    {
        public List<List<Node>> FindConnectedComponents(IInternalClassGraph graph)
        {
            var components = new List<List<Node>>();
            var visited = new HashSet<Node>();

            foreach (var node in graph.GetNodes())
            {
                if (visited.Contains(node))
                    continue;

                components.Add(ConstructComponent(node, graph, visited));
            }

            return components;
        }

        private List<Node> ConstructComponent(Node startNode,
            IInternalClassGraph graph,
            HashSet<Node> visited)
        {
            var dfsStack = new Stack<Node>();
            dfsStack.Push(startNode);

            var componentNodes = new List<Node>();
            while (dfsStack.Any())
            {
                var current = dfsStack.Pop();
                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                componentNodes.Add(current);

                foreach (var reference in graph.GetNeighbors(current))
                    dfsStack.Push(reference);
            }

            return componentNodes;
        }
    }
}
