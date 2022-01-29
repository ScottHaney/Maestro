using Autofac.Extras.Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class BigClassHelperTests
    {
        [Test]
        public void Returns_Empty_Tree_For_An_Empty_Class()
        {
            var emptyClass = @"public class Test {}";

            using (var mock = AutoMock.GetLoose())
            {
                var instance = mock.Create<InternalClassGraphGenerator>();

                var result = instance.CreateGraph(emptyClass, true);
                var expectedResult = new InternalClassGraph(new List<InternalClassNode>(), new InternalClassNodeAdjacencyMatrix(new Dictionary<InternalClassNode, HashSet<InternalClassNode>>()));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [Test]
        public void Finds_References_To_A_Field_Used_In_A_Method()
        {
            var emptyClass = @"public class Test { public readonly int Field; public void TestMethod() { return Field + 1; }}";

            using (var mock = AutoMock.GetLoose())
            {
                var instance = mock.Create<InternalClassGraphGenerator>();

                var result = instance.CreateGraph(emptyClass, true);

                var expectedMethods = new List<InternalClassNode>() { new InternalClassNode("TestMethod", InternalClassNodeType.Function) };
                var expectedVariables = new List<InternalClassNode>() { new InternalClassNode("Field", InternalClassNodeType.Variable) };
                var adjacenciesMap = new Dictionary<InternalClassNode, HashSet<InternalClassNode>>()
                {
                    { expectedMethods.First(), new HashSet<InternalClassNode>() { expectedVariables.First() } }
                };

                var expectedResult = new InternalClassGraph(expectedMethods.Concat(expectedVariables).ToList(), new InternalClassNodeAdjacencyMatrix(adjacenciesMap));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [Test]
        public void Handles_A_Class_With_No_References_Between_Variables_And_Methods()
        {
            var emptyClass = @"public class Test { public readonly int Field; public void TestMethod() { 1; }}";

            using (var mock = AutoMock.GetLoose())
            {
                var instance = mock.Create<InternalClassGraphGenerator>();

                var result = instance.CreateGraph(emptyClass, true);

                var expectedMethods = new List<InternalClassNode>() { new InternalClassNode("TestMethod", InternalClassNodeType.Function) };
                var expectedVariables = new List<InternalClassNode>() { new InternalClassNode("Field", InternalClassNodeType.Variable) };
                var adjacenciesMap = new Dictionary<InternalClassNode, HashSet<InternalClassNode>>();

                var expectedResult = new InternalClassGraph(expectedMethods.Concat(expectedVariables).ToList(), new InternalClassNodeAdjacencyMatrix(adjacenciesMap));

                Assert.AreEqual(expectedResult, result);
            }
        }

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