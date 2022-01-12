using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public abstract class BaseInternalClassGraph
    {
        public abstract IEnumerable<InternalClassNodePair> GetEdges();

        public abstract IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node);
    }
}
