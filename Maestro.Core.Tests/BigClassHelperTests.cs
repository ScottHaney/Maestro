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
    }
}