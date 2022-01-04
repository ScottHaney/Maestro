using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core
{
    public class InternalClassNodeAdjacencyMatrix : IEquatable<InternalClassNodeAdjacencyMatrix>
    {
        private Dictionary<InternalClassNode, HashSet<InternalClassNode>> _map;

        public InternalClassNodeAdjacencyMatrix(Dictionary<InternalClassNode, HashSet<InternalClassNode>> map)
        {
            _map = map;
        }

        public IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node)
        {
            if (_map.TryGetValue(node, out var matches))
                return matches;
            else
                return Enumerable.Empty<InternalClassNode>();
        }

        public static bool operator ==(InternalClassNodeAdjacencyMatrix lhs, InternalClassNodeAdjacencyMatrix rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(InternalClassNodeAdjacencyMatrix lhs, InternalClassNodeAdjacencyMatrix rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(InternalClassNodeAdjacencyMatrix other)
        {
            if (_map.Count != other._map.Count)
                return false;

            foreach (var item in _map)
            {
                if (other._map.TryGetValue(item.Key, out var otherValue))
                {
                    if (!new HashSet<InternalClassNode>(item.Value).SetEquals(new HashSet<InternalClassNode>(otherValue)))
                        return false;
                }
                else
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InternalClassNodeAdjacencyMatrix);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                foreach (var entry in _map)
                {
                    result += entry.Key.GetHashCode();
                    foreach (var item in entry.Value)
                        result += item.GetHashCode();
                }

                return result;
            }
        }
    }
}
