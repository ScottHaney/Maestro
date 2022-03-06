using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public interface IInternalClassGraph
    {
        IEnumerable<Node> GetNodes();
        IEnumerable<Node> GetNeighbors(Node node);
    }
}
