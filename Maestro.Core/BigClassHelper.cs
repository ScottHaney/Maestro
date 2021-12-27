﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maestro.Core
{
    public class BigClassHelper
    {
        public InternalClassDiagram CreateDiagram(string csFileWithClass)
        {
            var tree = CSharpSyntaxTree.ParseText(csFileWithClass);
            var root = tree.GetCompilationUnitRoot();

            var variables = GetVariableNodes(root).ToList();
            var methods = GetFunctionNodes(root, variables).ToList();

            return new InternalClassDiagram(variables, methods);
        }

        private IEnumerable<VariableNode> GetVariableNodes(CompilationUnitSyntax root)
        {
            var classNode = root.ChildNodes().OfType<ClassDeclarationSyntax>().Single();
            var fields = classNode.ChildNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var variable = field.Declaration.Variables.Single();
                yield return new VariableNode(variable.Identifier.ValueText);
            }
        }

        private IEnumerable<FunctionNode> GetFunctionNodes(CompilationUnitSyntax root, IEnumerable<VariableNode> variables)
        {
            var classNode = root.ChildNodes().OfType<ClassDeclarationSyntax>().Single();
            var methods = classNode.ChildNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var references = GetReferencedVariables(method, variables).ToList();
                yield return new FunctionNode(method.Identifier.ValueText, references);
            }
        }

        private IEnumerable<VariableNode> GetReferencedVariables(MethodDeclarationSyntax method, IEnumerable<VariableNode> nodes)
        {
            var refs = method.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();

            var names = new HashSet<string>(refs.Select(x => x.Identifier.ValueText).Distinct());
            return nodes.Where(x => names.Contains(x.Name));
        }
    }

    public class InternalClassDiagram : IEquatable<InternalClassDiagram>
    {
        public readonly List<VariableNode> VariableNodes;
        public readonly List<FunctionNode> FunctionNodes;

        public bool IsEmpty => !VariableNodes.Any() && !FunctionNodes.Any();

        public InternalClassDiagram(List<VariableNode> variableNodes,
            List<FunctionNode> functionNodes)
        {
            VariableNodes = variableNodes;
            FunctionNodes = functionNodes;
        }

        public static bool operator==(InternalClassDiagram lhs, InternalClassDiagram rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(InternalClassDiagram lhs, InternalClassDiagram rhs)
            => !(lhs == rhs);

        public bool Equals(InternalClassDiagram other)
        {
            if (other == null)
                return false;

            return VariableNodes.SequenceEqual(other.VariableNodes) && FunctionNodes.SequenceEqual(other.FunctionNodes);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InternalClassDiagram);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                foreach (var node in VariableNodes)
                    result += node.GetHashCode();

                foreach (var node in FunctionNodes)
                    result += node.GetHashCode();

                return result;
            }
        }
    }

    public class VariableNode : IEquatable<VariableNode>
    {
        public readonly string Name;
        
        public VariableNode(string name)
        {
            Name = name ?? string.Empty;
        }

        public static bool operator==(VariableNode lhs, VariableNode rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(VariableNode lhs, VariableNode rhs)
            => !(lhs == rhs);

        public bool Equals(VariableNode other)
        {
            if (other == null)
                return false;

            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as VariableNode);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class FunctionNode : IEquatable<FunctionNode>
    {
        public readonly string Name;
        public readonly List<VariableNode> References;

        public FunctionNode(string name,
            List<VariableNode> references)
        {
            Name = name;
            References = references;
        }

        public static bool operator==(FunctionNode lhs, FunctionNode rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator !=(FunctionNode lhs, FunctionNode rhs)
            => !(lhs == rhs);

        public bool Equals(FunctionNode other)
        {
            if (other == null)
                return false;

            return Name == other.Name && References.SequenceEqual(other.References);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FunctionNode);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = Name?.GetHashCode() ?? 11;
                foreach (var reference in (References ?? Enumerable.Empty<VariableNode>()))
                    result += reference.GetHashCode();

                return result;
            }
        }
    }



    public class InternalClassGraphBuilder
    {
        private readonly List<InternalClassNode> _nodes;
        private readonly InternalClassNodeAdjacencyMatrix _adjacencyMatrix;

        public InternalClassGraphBuilder(List<InternalClassNode> nodes)
        {
            _nodes = nodes;
            _adjacencyMatrix = new InternalClassNodeAdjacencyMatrix();
        }

        public void AddAdjacency(InternalClassNode source, List<InternalClassNode> neighbors)
        {
            _adjacencyMatrix.AddNeighbors(source, neighbors);
        }

        public InternalClassGraph Build()
        {
            return new InternalClassGraph(_nodes, _adjacencyMatrix);
        }
    }

    public class InternalClassNodeAdjacencyMatrix
    {
        private Dictionary<InternalClassNode, HashSet<InternalClassNode>> _map;

        public IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node)
        {
            if (_map.TryGetValue(node, out var matches))
                return matches;
            else
                return Enumerable.Empty<InternalClassNode>();
        }

        public void AddNeighbors(InternalClassNode source, List<InternalClassNode> neighbors)
        {
            if (_map.TryGetValue(source, out var values))
            {
                foreach (var neighbor in neighbors)
                    values.Add(neighbor);
            }
            else
                _map[source] = new HashSet<InternalClassNode>(neighbors);
        }

        public bool AreTheSame(InternalClassNodeAdjacencyMatrix other)
        {
            if (_map.Count != other._map.Count)
                return false;

            foreach (var item in _map)
            {
                if (other._map.TryGetValue(item.Key, out var otherValue))
                {
                    if (!item.Value.SequenceEqual(otherValue))
                        return false;
                }
                else
                    return false;
            }

            return true;
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

    public class InternalClassGraph : IEquatable<InternalClassGraph>
    {
        public readonly List<InternalClassNode> Nodes;
        private readonly InternalClassNodeAdjacencyMatrix _adjacencyMatrix;

        public InternalClassGraph(List<InternalClassNode> nodes,
            InternalClassNodeAdjacencyMatrix adjacencyMatrix)
        {
            Nodes = nodes;
            _adjacencyMatrix = adjacencyMatrix;
        }

        public IEnumerable<InternalClassNode> GetNeighbors(InternalClassNode node)
        {
            return _adjacencyMatrix.GetNeighbors(node);
        }

        public bool Equals(InternalClassGraph other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Nodes.SequenceEqual(other.Nodes) && _adjacencyMatrix.AreTheSame(other._adjacencyMatrix);
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

        public static bool operator==(InternalClassNode lhs, InternalClassNode rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator!=(InternalClassNode lhs, InternalClassNode rhs)
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
