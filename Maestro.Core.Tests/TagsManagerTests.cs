using NUnit.Framework;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.IO;
using Newtonsoft.Json;

namespace Maestro.Core.Tests
{
    public class Tests
    {
        [Test]
        public void Adds_A_Tag_To_A_File()
        {
            var projectFilePath = @"c:\code\MyApp\Project1\Project1.csproj";
            var solutionFilePath = @"c:\code\MyApp\MyProject.sln";

            var csFilePath = Path.Combine(Path.GetDirectoryName(projectFilePath), "Class1.cs");
            var projectItem = new ProjectItem(csFilePath, projectFilePath, solutionFilePath);

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { projectFilePath, new MockFileData("") },
                { csFilePath, new MockFileData("") }
            });

            var tagsManager = new TagsManager(fileSystem, solutionFilePath);
            tagsManager.AddItem(projectItem, new Tag("TestTag"));

            var linkFilePath = @"C:\code\MyApp\Project1\__Tags\TestTag\Class1.cs.link";
            Assert.IsTrue(fileSystem.FileExists(linkFilePath));

            var linkFileContents = JsonConvert.DeserializeObject<LinkFileContent>(fileSystem.File.ReadAllText(linkFilePath));
            Assert.AreEqual(@"Project1\Class1.cs", linkFileContents.RelativeFilePath);
            Assert.AreEqual(@"Project1\Project1.csproj", linkFileContents.ProjectIdentifier.RelativeProjectFilePath);
        }
    }
}