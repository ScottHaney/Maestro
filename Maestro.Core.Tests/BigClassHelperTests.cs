using Autofac;
using Autofac.Extras.Moq;
using Maestro.Core.CodingConstructs.Classes.Graphs;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class BigClassHelperTests
    {
        [Test]
        public void Finds_Two_Graph_Components_In_A_Class_That_Has_Two_Distinct_Components()
        {
            var emptyClass = @"public class Test { public readonly int Field1; public readonly int Field2; public int TestMethod1() { return Field1; } public int TestMethod2() { return Field2; } }";

            using (var mock = AutoMock.GetLoose())
            {
                var instance = mock.Create<ClassManager>();
                var components = instance.FindConnectedComponents(emptyClass);

                Assert.AreEqual(2, components.Count);
                Assert.IsTrue(components.Items.All(x => x.Nodes.Count == 2));
            }
        }
    }
}