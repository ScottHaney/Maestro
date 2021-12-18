using Autofac.Extras.Moq;
using NUnit.Framework;

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
                Assert.IsFalse(result.IsEmpty);
            }
        }
    }
}