using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using TestUtils;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class UnitTestFinderTests
    {
        [Test]
        public void Finds_Unit_Test_File_When_It_Is_There()
        {
            var workspace = new TestWorkspaceBuilder()
                .AddProject("Project")
                .AddDocument("Project", "Class1.cs", "")
                .AddProject("Project.Tests")
                .AddDocument("Project.Tests", "Class1Tests.cs", "")
                .Build();

            var unitTestFinder = new UnitTestFinder(workspace);

            var implementationDocument = workspace.CurrentSolution
                .Projects.First(x => x.Name == "Project")
                .Documents.First(x => x.Name == "Class1.cs");

            var testFile = unitTestFinder.FindUnitTest(implementationDocument);

            Assert.AreEqual("Class1Tests.cs", testFile.Name);
        }
    }
}
