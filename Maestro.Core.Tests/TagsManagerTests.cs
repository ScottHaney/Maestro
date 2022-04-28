using NUnit.Framework;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.IO;

namespace Maestro.Core.Tests
{
    public class Tests
    {
        [Test]
        public void Adds_A_Tag_To_A_File()
        {
            var project = new Project(@"c:\code\MyApp\Project1\Project1.csproj");

            var csFilePath = Path.Combine(project.FolderPath, "Class1.cs");
            var projectItem = new ProjectItem(csFilePath, project);

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { project.ProjectFilePath, new MockFileData("") },
                { csFilePath, new MockFileData("") }
            });

            var tagsManager = new TagsManager(fileSystem);
            tagsManager.AddItem(project.GetTagsFolderPath(), projectItem, new Tag("TestTag"));

            var linkFilePath = @"C:\code\MyApp\Project1\__Tags\TestTag\Class1.cs.link";
            Assert.IsTrue(fileSystem.FileExists(linkFilePath));
            Assert.AreEqual("Class1.cs", fileSystem.File.ReadAllText(linkFilePath));
        }
    }
}