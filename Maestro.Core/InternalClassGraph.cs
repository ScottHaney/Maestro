using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class InternalClassGraph : BaseInternalClassGraph, IEquatable<InternalClassGraph>
    {
        public readonly List<InternalClassNode> Nodes;
        private readonly InternalClassNodeAdjacencyMatrix _adjacencyMatrix;

        public InternalClassGraph(List<InternalClassNode> nodes,
            InternalClassNodeAdjacencyMatrix adjacencyMatrix)
        {
            Nodes = nodes;
            _adjacencyMatrix = adjacencyMatrix;
        }

        public override IEnumerable<InternalClassNodePair> GetEdges()
        {
            return _adjacencyMatrix.GetEdges();
        }

        public override IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node)
        {
            return _adjacencyMatrix.GetNeighbors(node);
        }

        public override bool Equals(BaseInternalClassGraph other)
        {
            return Equals(other as InternalClassGraph);
        }

        public bool Equals(InternalClassGraph other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return new HashSet<InternalClassNode>(Nodes).SetEquals(new HashSet<InternalClassNode>(other.Nodes)) && _adjacencyMatrix.Equals(other._adjacencyMatrix);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InternalClassGraph);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                foreach (var node in Nodes)
                    result += node.GetHashCode();

                result += _adjacencyMatrix.GetHashCode();
                return result;
            }
        }
    }

    public class InternalClassNodePair : IEquatable<InternalClassNodePair>
    {
        public readonly InternalClassNode Node1;
        public readonly InternalClassNode Node2;

        public InternalClassNodePair(InternalClassNode node1,
            InternalClassNode node2)
        {
            Node1 = node1;
            Node2 = node2;
        }

        public bool HasNode(InternalClassNode node)
        {
            return node == Node1 || node == Node2;
        }

        public static bool operator==(InternalClassNodePair lhs, InternalClassNodePair rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(InternalClassNodePair lhs, InternalClassNodePair rhs)
            => !(lhs == rhs);

        public bool Equals(InternalClassNodePair other)
        {
            if (ReferenceEquals(other, null))
                return false;

            var hash = new HashSet<InternalClassNode>() { Node1, Node2 };
            if (hash.Contains(other.Node1))
                hash.Remove(other.Node1);
            else
                return false;

            return hash.Contains(other.Node2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InternalClassNodePair);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Node1.GetHashCode() + Node2.GetHashCode();
            }
        }
    }


    public class InternalClassNode : IEquatable<InternalClassNode>
    {
        public readonly string Name;
        public readonly InternalClassNodeType Type;

        public InternalClassNode(string name,
            InternalClassNodeType type)
        {
            Name = name;
            Type = type;
        }

        public static bool operator ==(InternalClassNode lhs, InternalClassNode rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(InternalClassNode lhs, InternalClassNode rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(InternalClassNode other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Name == other.Name && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InternalClassNode);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Name?.GetHashCode() ?? 3 + Type.GetHashCode();
            }
        }
    }

    public enum InternalClassNodeType
    {
        Variable,
        Function
    }
}
