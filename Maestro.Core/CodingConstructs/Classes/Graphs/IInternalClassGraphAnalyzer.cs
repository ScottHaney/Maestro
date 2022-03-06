using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public interface IInternalClassGraphAnalyzer
    {
        List<List<Node>> FindConnectedComponents(IInternalClassGraph graph);
    }
}
