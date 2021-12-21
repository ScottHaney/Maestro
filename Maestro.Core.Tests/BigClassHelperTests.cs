using Autofac.Extras.Moq;
using NUnit.Framework;
using System.Collections.Generic;

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
                var instance = mock.Create<BigClassHelper>();

                var result = instance.CreateDiagram(emptyClass);
                Assert.IsTrue(result.IsEmpty);
            }
        }

        [Test]
        public void Finds_References_To_A_Field_Used_In_A_Method()
        {
            var emptyClass = @"public class Test { public readonly int Field; public void TestMethod() { return Field + 1; }}";

            using (var mock = AutoMock.GetLoose())
            {
                var instance = mock.Create<BigClassHelper>();

                var result = instance.CreateDiagram(emptyClass);

                var expectedVariables = new List<VariableNode>() { new VariableNode("Field") };
                var expected = new InternalClassDiagram(expectedVariables, new List<FunctionNode>() { new FunctionNode("TestMethod", expectedVariables) });

                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void Handles_A_Class_With_No_References_Between_Variables_And_Methods()
        {
            var emptyClass = @"public class Test { public readonly int Field; public void TestMethod() { 1; }}";

            using (var mock = AutoMock.GetLoose())
            {
                var instance = mock.Create<BigClassHelper>();

                var result = instance.CreateDiagram(emptyClass);
                var expected = new InternalClassDiagram(new List<VariableNode>() { new VariableNode("Field") }, new List<FunctionNode>() { new FunctionNode("TestMethod", new List<VariableNode>()) });

                Assert.AreEqual(expected, result);
            }
        }
    }
}