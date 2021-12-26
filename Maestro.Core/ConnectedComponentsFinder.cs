using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class ConnectedComponentsFinder
    {
        public List<ConnectedComponent> Find(InternalClassDiagram diagram)
        {
            throw new NotImplementedException();
        }

        private void DFS(InternalClassDiagram diagram)
        {

        }
    }

    public class ConnectedComponent
    {
        public readonly List<FunctionNode> Nodes;

        public ConnectedComponent(List<FunctionNode> nodes)
        {
            Nodes = nodes;
        }
    }
}
