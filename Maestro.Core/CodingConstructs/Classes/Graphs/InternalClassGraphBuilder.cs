using Maestro.Core.CodingConstructs.Classes.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public class InternalClassGraphBuilder : IInternalClassGraphBuilder
    {
        private readonly ICSharpClassParser _parser;

        public InternalClassGraphBuilder(ICSharpClassParser parser)
        {
            _parser = parser;
        }

        public IInternalClassGraph Build()
        {
            var adjacencyList = new Dictionary<Node, List<Node>>();

            var variablesMap = ProcessVariables(_parser.GetVariableNames(), adjacencyList);
            ProcessMethods(_parser.GetMethodsInfo(), variablesMap, adjacencyList);

            return new InternalClassGraph(adjacencyList);
        }

        private Dictionary<string, VariableNode> ProcessVariables(IEnumerable<string> variableNames,
            Dictionary<Node, List<Node>> adjacencyList)
        {
            var variablesMap = new Dictionary<string, VariableNode>();
            foreach (var variableName in variableNames)
            {
                var variableNode = new VariableNode(variableName);

                variablesMap[variableName] = variableNode;
                adjacencyList[variableNode] = new List<Node>();
            }

            return variablesMap;
        }

        private void ProcessMethods(IEnumerable<MethodReferences> methodReferences,
            Dictionary<string, VariableNode> variablesMap,
            Dictionary<Node, List<Node>> adjacencyList)
        {
            var methodsMap = ProcessMethodNames(methodReferences,
                adjacencyList);

            foreach (var methodReference in methodReferences)
                AddMethodLinks(methodReference, methodsMap, variablesMap, adjacencyList);
        }

        private Dictionary<string, MethodNode> ProcessMethodNames(IEnumerable<MethodReferences> methodReferences,
            Dictionary<Node, List<Node>> adjacencyList)
        {
            var methodsMap = new Dictionary<string, MethodNode>();
            foreach (var methodReference in methodReferences)
            {
                var methodNode = new MethodNode(methodReference.MethodName);

                methodsMap[methodReference.MethodName] = methodNode;
                adjacencyList[methodNode] = new List<Node>();
            }

            return methodsMap;
        }

        private void AddMethodLinks(MethodReferences methodReference,
            Dictionary<string, MethodNode> methodsMap,
            Dictionary<string, VariableNode> variablesMap,
            Dictionary<Node, List<Node>> adjacencyList)
        {
            var methodNode = methodsMap[methodReference.MethodName];

            foreach (var variableName in methodReference.ReferencedVariableNames)
            {
                adjacencyList[methodNode].Add(variablesMap[variableName]);
            }

            foreach (var calledMethodName in methodReference.CalledMethodNames)
            {
                adjacencyList[methodNode].Add(methodsMap[calledMethodName]);
            }
        }
    }
}
