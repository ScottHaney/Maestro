using Autofac;
using Autofac.Extras.Moq;
using Maestro.Core.CodingConstructs.Classes.Graphs;
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
        public void An_Empty_Class_Produces_An_Empty_Graph()
        {
            var emptyClass = @"public class Test {}";

            using (var mock = AutoMock.GetLoose(cb =>
            {
                cb.RegisterType<CSharpClassParserFactory>().As<ICSharpClassParserFactory>();
                cb.RegisterType<Maestro.Core.CodingConstructs.Classes.Graphs.InternalClassGraphBuilder>().As<IInternalClassGraphBuilder>();
            }))
            {
                var factory = mock.Create<ICSharpClassParserFactory>();
                var parser = factory.CreateParser(emptyClass);

                var builder = mock.Create<IInternalClassGraphBuilder>(new TypedParameter(typeof(ICSharpClassParser), parser));

                var graph = builder.Build();

                Assert.AreEqual(0, graph.GetNodes().Count());
            }
        }
    }
}
