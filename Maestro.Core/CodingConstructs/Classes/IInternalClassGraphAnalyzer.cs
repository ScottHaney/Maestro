using Maestro.Core.CodingConstructs.Classes.Graphs;
using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes
{
    public interface IInternalClassGraphAnalyzer
    {
        List<List<Node>> FindConnectedComponents(IInternalClassGraph graph);
    }
}
