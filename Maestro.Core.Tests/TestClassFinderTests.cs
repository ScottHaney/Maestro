using Maestro.Core.Links;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Core.Tests
{
    public class TestClassFinderTests
    {
        [Test]
        public void Finds_Test_Class_That_Matches_Naming_Convention()
        {
            var builder = new TestWorkspaceBuilder(@"C:\Code\CodeProject\solution.sln");

            var codeProjectName = "Project1.csproj";
            builder.AddProject(codeProjectName);
            builder.AddDocument(codeProjectName, "Class1.cs", "");

            var testProjectName = "Project1.Tests.csproj";
            builder.AddProject(testProjectName);
            builder.AddDocument(testProjectName, "Class1Tests.cs", "");

            var workspace = builder.Build();

            var testClassFinder = new TestClassFinder(workspace);
            var testClass = testClassFinder.GetTestClass(new ProjectItem(codeProjectName, "Class1.cs"));

            Assert.AreEqual(testClass, new ProjectItem(testProjectName, "Class1Tests.cs"));
        }
    }
}
