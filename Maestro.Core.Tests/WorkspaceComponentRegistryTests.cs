using Maestro.Core.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtils;

namespace Maestro.Core.Tests
{
    public class WorkspaceComponentRegistryTests
    {
        [Test]
        public async Task Gets_Components_From_Single_File_With_No_Build_Errors()
        {
            var workspace = TestHelpers.CreateSingleDocumentWorkspace(@"TestCsFiles/WorkspaceComponentRegistryTestFileThatCompiles.cs");
            var registry = new WorkspaceComponentRegistry(workspace);

            var components = await registry.GetComponentsAsync();

            CollectionAssert.AreEquivalent(new[] { "Component1", "Component2", "IComponent" }, components.Select(x => x.Name));
        }

        [Test]
        public async Task Gets_Components_From_Single_File_With_Build_Errors()
        {
            var workspace = TestHelpers.CreateSingleDocumentWorkspace(@"TestCsFiles/WorkspaceComponentRegistryTestFileThatDoesNotCompile.cs");
            var registry = new WorkspaceComponentRegistry(workspace);

            var components = await registry.GetComponentsAsync();

            CollectionAssert.AreEquivalent(new[] { "Component1", "Component2" }, components.Select(x => x.Name));
        }
    }
}
