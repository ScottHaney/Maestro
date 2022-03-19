using Autofac;
using Autofac.Extras.Moq;
using Maestro.Core.CodingConstructs.Classes.Graphs;
using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using Maestro.Core.CodingConstructs.Classes.Parsing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core.Tests
{
    public class InternalClassGraphConstructionTests
    {
        [Test]
        public void Empty_Class()
        {
            var classText = @"public class Test {}";

            using (var mock = AutoMock.GetLoose(cb =>
            {
                cb.RegisterType<CSharpClassParserFactory>().As<ICSharpClassParserFactory>();
                cb.RegisterType<Maestro.Core.CodingConstructs.Classes.Graphs.InternalClassGraphBuilder>().As<IInternalClassGraphBuilder>();
            }))
            {
                var factory = mock.Create<ICSharpClassParserFactory>();
                var parser = factory.CreateParser(classText);

                var builder = mock.Create<IInternalClassGraphBuilder>(new TypedParameter(typeof(ICSharpClassParser), parser));

                var graph = builder.Build();

                Assert.AreEqual(0, graph.GetNodes().Count());
            }
        }

        [Test]
        public void Single_Method_Referencing_A_Single_Variable()
        {
            var classText = @"public class Test { public readonly int Field; public void TestMethod() { return Field + 1; }}";

            using (var mock = AutoMock.GetLoose(cb =>
            {
                cb.RegisterType<CSharpClassParserFactory>().As<ICSharpClassParserFactory>();
                cb.RegisterType<Maestro.Core.CodingConstructs.Classes.Graphs.InternalClassGraphBuilder>().As<IInternalClassGraphBuilder>();
            }))
            {
                var factory = mock.Create<ICSharpClassParserFactory>();
                var parser = factory.CreateParser(classText);

                var builder = mock.Create<IInternalClassGraphBuilder>(new TypedParameter(typeof(ICSharpClassParser), parser));

                var actualGraph = builder.Build();

                var methodNode = new MethodNode("TestMethod");
                var variableNode = new VariableNode("Field");

                var expectedGraph = new InternalClassGraph(
                    new Dictionary<Node, List<Node>>()
                    {
                        { methodNode, new List<Node>() { variableNode } },
                        { variableNode, new List<Node>() { methodNode } }
                    });

                Assert.IsTrue(AreTheSameGraph(actualGraph, expectedGraph));
            }
        }

        [Test]
        public void Single_Method_And_Single_Variable_With_No_References_Between_Them()
        {
            var classText = @"public class Test { public readonly int Field; public void TestMethod() { 1; }}";

            using (var mock = AutoMock.GetLoose(cb =>
            {
                cb.RegisterType<CSharpClassParserFactory>().As<ICSharpClassParserFactory>();
                cb.RegisterType<Maestro.Core.CodingConstructs.Classes.Graphs.InternalClassGraphBuilder>().As<IInternalClassGraphBuilder>();
            }))
            {
                var factory = mock.Create<ICSharpClassParserFactory>();
                var parser = factory.CreateParser(classText);

                var builder = mock.Create<IInternalClassGraphBuilder>(new TypedParameter(typeof(ICSharpClassParser), parser));

                var actualGraph = builder.Build();

                var methodNode = new MethodNode("TestMethod");
                var variableNode = new VariableNode("Field");

                var expectedGraph = new InternalClassGraph(
                    new Dictionary<Node, List<Node>>()
                    {
                        { methodNode, new List<Node>() { } },
                        { variableNode, new List<Node>() { } }
                    });

                Assert.IsTrue(AreTheSameGraph(actualGraph, expectedGraph));
            }
        }

        [Test]
        public void Correctly_Parses_Class_With_An_External_Method_Call()
        {
            var classText = @"public class Test { public readonly int Field; public void TestMethod() { System.Console.WriteLine(""Test""); }}";

            using (var mock = AutoMock.GetLoose(cb =>
            {
                cb.RegisterType<CSharpClassParserFactory>().As<ICSharpClassParserFactory>();
                cb.RegisterType<Maestro.Core.CodingConstructs.Classes.Graphs.InternalClassGraphBuilder>().As<IInternalClassGraphBuilder>();
            }))
            {
                var factory = mock.Create<ICSharpClassParserFactory>();
                var parser = factory.CreateParser(classText);

                var builder = mock.Create<IInternalClassGraphBuilder>(new TypedParameter(typeof(ICSharpClassParser), parser));

                var actualGraph = builder.Build();

                var methodNode = new MethodNode("TestMethod");
                var variableNode = new VariableNode("Field");

                var expectedGraph = new InternalClassGraph(
                    new Dictionary<Node, List<Node>>()
                    {
                        { methodNode, new List<Node>() { } },
                        { variableNode, new List<Node>() { } }
                    });

                Assert.IsTrue(AreTheSameGraph(actualGraph, expectedGraph));
            }
        }

        private bool AreTheSameGraph(IInternalClassGraph graph1, IInternalClassGraph graph2)
        {
            var nodes1 = graph1.GetNodes().ToDictionary(x => x.Name, x => x);
            var nodes2 = graph2.GetNodes().ToDictionary(x => x.Name, x => x);

            if (nodes1.Count != nodes2.Count)
                return false;

            foreach (var node1Pair in nodes1)
            {
                if (!nodes2.ContainsKey(node1Pair.Key))
                    return false;

                var adjacencies1 = graph1.GetNeighbors(node1Pair.Value).Select(x => x.Name);
                var adjacencies2 = graph2.GetNeighbors(nodes2[node1Pair.Key]).Select(x => x.Name);

                if (!adjacencies1.SequenceEqual(adjacencies2))
                    return false;
            }

            return true;
        }
    }
}
