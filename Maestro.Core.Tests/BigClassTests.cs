using Autofac;
using Autofac.Extras.Moq;
using Maestro.Core.CodingConstructs.Classes;
using Maestro.Core.CodingConstructs.Classes.Graphs;
using Maestro.Core.CodingConstructs.Classes.Parsing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class BigClassTests
    {
        [Test]
        public void Splits_A_Class_With_Two_Distinct_Components_Into_Two()
        {
            var classText = @"public class Test { public readonly int Field1; public readonly int Field2; public int TestMethod1() { return Field1; } public int TestMethod2() { return Field2; } }";

            using (var mock = AutoMock.GetLoose(cb =>
            {
                cb.RegisterType<CSharpClassParserFactory>().As<ICSharpClassParserFactory>();
                cb.RegisterType<Maestro.Core.CodingConstructs.Classes.Graphs.InternalClassGraphBuilder>().As<IInternalClassGraphBuilder>();
                cb.RegisterType<InternalClassGraphAnalyzer>().As<IInternalClassGraphAnalyzer>();
            }))
            {
                var factory = mock.Create<ICSharpClassParserFactory>();
                var parser = factory.CreateParser(classText);

                var builder = mock.Create<IInternalClassGraphBuilder>(new TypedParameter(typeof(ICSharpClassParser), parser));

                var actualGraph = builder.Build();
                var analyzer = mock.Create<IInternalClassGraphAnalyzer>();

                var components = analyzer.FindConnectedComponents(actualGraph);

                Assert.AreEqual(2, components.Count);
            }
        }
    }
}