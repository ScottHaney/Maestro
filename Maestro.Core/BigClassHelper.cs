using Microsoft.CodeAnalysis.CSharp;
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

        public InternalClassGraph CreateDiagram2(string csFileWithClass)
        {
            var tree = CSharpSyntaxTree.ParseText(csFileWithClass);
            var root = tree.GetCompilationUnitRoot();

            var variables = GetVariableNodes2(root).ToList();
            var methodsMap = GetFunctionNodes2(root, variables);

            var allNodes = variables.Concat(methodsMap.Keys).ToList();
            var builder = new InternalClassGraphBuilder(allNodes);

            foreach (var entry in methodsMap)
                builder.AddAdjacency(entry.Key, entry.Value);

            return builder.Build();
        }

        private IEnumerable<InternalClassNode> GetVariableNodes2(CompilationUnitSyntax root)
        {
            var classNode = root.ChildNodes().OfType<ClassDeclarationSyntax>().Single();
            var fields = classNode.ChildNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var variable = field.Declaration.Variables.Single();
                yield return new InternalClassNode(variable.Identifier.ValueText, InternalClassNodeType.Variable);
            }
        }

        private Dictionary<InternalClassNode, List<InternalClassNode>> GetFunctionNodes2(CompilationUnitSyntax root, List<InternalClassNode> variableNodes)
        {
            var classNode = root.ChildNodes().OfType<ClassDeclarationSyntax>().Single();
            var methods = classNode.ChildNodes().OfType<MethodDeclarationSyntax>();

            var result = new Dictionary<InternalClassNode, List<InternalClassNode>>();
            foreach (var method in methods)
            {
                var variableRefs = GetReferencedVariables2(method, variableNodes);
                var methodNode = new InternalClassNode(method.Identifier.ValueText, InternalClassNodeType.Function);

                result[methodNode] = variableRefs.ToList();
            }

            return result;
        }

        private IEnumerable<InternalClassNode> GetReferencedVariables2(MethodDeclarationSyntax method, IEnumerable<InternalClassNode> nodes)
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
        private readonly InternalClassNodeAdjacencyMatrixBuilder _adjacencyMatrixBuilder;

        public InternalClassGraphBuilder(List<InternalClassNode> nodes)
        {
            _nodes = nodes;
            _adjacencyMatrixBuilder = new InternalClassNodeAdjacencyMatrixBuilder();
        }

        public void AddAdjacency(InternalClassNode source, List<InternalClassNode> neighbors)
        {
            _adjacencyMatrixBuilder.AddNeighbors(source, neighbors);
        }

        public InternalClassGraph Build()
        {
            return new InternalClassGraph(_nodes, _adjacencyMatrixBuilder.Build());
        }
    }

    public class InternalClassNodeAdjacencyMatrixBuilder
    {
        private Dictionary<InternalClassNode, HashSet<InternalClassNode>> _map = new Dictionary<InternalClassNode, HashSet<InternalClassNode>>();
        
        public void AddNeighbors(InternalClassNode source, List<InternalClassNode> neighbors)
        {
            if (_map.TryGetValue(source, out var values))
            {
                foreach (var neighbor in neighbors)
                    values.Add(neighbor);
            }
            else if (neighbors.Any())
                _map[source] = new HashSet<InternalClassNode>(neighbors);
        }

        public InternalClassNodeAdjacencyMatrix Build()
        {
            return new InternalClassNodeAdjacencyMatrix(_map);
        }
    }

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

        public static bool operator==(InternalClassNodeAdjacencyMatrix lhs, InternalClassNodeAdjacencyMatrix rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator!=(InternalClassNodeAdjacencyMatrix lhs, InternalClassNodeAdjacencyMatrix rhs)
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
