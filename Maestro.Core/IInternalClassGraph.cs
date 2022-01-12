using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public interface IInternalClassGraph
    {
        IEnumerable<InternalClassNodePair> GetEdges();

        IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node);
    }
}
