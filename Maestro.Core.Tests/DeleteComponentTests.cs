using Maestro.Core.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class DeleteComponentTests
    {
        [Test]
        public async Task Deletes_Single_Component()
        {
            var workspace = SingleTestDocumentWorkspace.Create(@"TestCsFiles/DeleteComponentTestFile.cs");
            var registry = workspace.GetRegistry();

            var componentManager = new ComponentManager();
            var allComponents = (await registry.GetComponentsAsync()).ToList();

            var match = allComponents.First(x => x.Name == "Component2");

            await componentManager.DeleteComponentAsync(match);

            var updatedSyntaxTree = workspace.GetSyntaxTree();

            var remainingComponents = (await registry.GetComponentsAsync()).ToList();

            CollectionAssert.AreEquivalent(new[] { "Component1" }, remainingComponents.Select(x => x.Name));
        }
    }
}
