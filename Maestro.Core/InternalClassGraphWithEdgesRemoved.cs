using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core
{
    public class InternalClassGraphWithEdgesRemoved : BaseInternalClassGraph, IEquatable<InternalClassGraphWithEdgesRemoved>
    {
        private readonly BaseInternalClassGraph _wrappedClassGraph;
        private readonly HashSet<InternalClassNodePair> _removedEdges;

        public override List<InternalClassNode> Nodes => _wrappedClassGraph.Nodes;

        public InternalClassGraphWithEdgesRemoved(BaseInternalClassGraph wrappedClassGraph,
            HashSet<InternalClassNodePair> removedEdges)
        {
            _wrappedClassGraph = wrappedClassGraph;
            _removedEdges = removedEdges;
        }

        public override IEnumerable<InternalClassNodePair> GetEdges()
        {
            return _wrappedClassGraph.GetEdges()
                .Where(x => !_removedEdges.Contains(x));
        }

        public override IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node)
        {
            foreach (var neighbor in _wrappedClassGraph.GetNeighbors(node))
            {
                var edge = new InternalClassNodePair(neighbor, node);
                if (!_removedEdges.Contains(edge))
                    yield return neighbor;
            }
        }

        public static bool operator==(InternalClassGraphWithEdgesRemoved lhs, InternalClassGraphWithEdgesRemoved rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator!=(InternalClassGraphWithEdgesRemoved lhs, InternalClassGraphWithEdgesRemoved rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(InternalClassGraphWithEdgesRemoved other)
        {
            if (ReferenceEquals(other, null))
                return false;

            var thisEdges = new HashSet<InternalClassNodePair>(GetEdges());
            var otherEdges = new HashSet<InternalClassNodePair>(other.GetEdges());

            return thisEdges.SetEquals(otherEdges);
        }

        public override bool Equals(BaseInternalClassGraph other)
        {
            return Equals(other as InternalClassGraphWithEdgesRemoved);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InternalClassGraphWithEdgesRemoved);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var sum = 0;
                foreach (var edge in GetEdges())
                    sum += edge.GetHashCode();

                return sum;
            }
        }
    }
}
