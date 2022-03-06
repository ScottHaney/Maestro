using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    /// <summary>
    /// Graph where methods/fields are nodes and edges created whenever a method uses a field or calls another method.
    /// </summary>
    public interface IInternalClassGraph
    {
        IEnumerable<Node> GetNodes();
        IEnumerable<Node> GetNeighbors(Node node);
    }
}
