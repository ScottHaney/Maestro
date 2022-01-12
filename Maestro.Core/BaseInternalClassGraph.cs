using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public abstract class BaseInternalClassGraph : IEquatable<BaseInternalClassGraph>
    {
        public abstract List<InternalClassNode> Nodes { get; }

        public abstract bool Equals(BaseInternalClassGraph other);

        public abstract IEnumerable<InternalClassNodePair> GetEdges();

        public abstract IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node);
    }
}
