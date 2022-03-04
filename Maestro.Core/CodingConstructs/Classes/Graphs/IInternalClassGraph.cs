using Maestro.Core.CodingConstructs.Classes.Architecture;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public interface IInternalClassGraph
    {
        IEnumerable<Node> GetNodes();
    }
}
