using Maestro.Core.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Core.Tests
{
    public class WorkspaceComponentRegistryTests
    {
        [Test]
        public async Task Gets_Classes_From_Single_File_With_No_Build_Errors()
        {
            var workspace = TestUtils.CreateSingleDocumentWorkspace(@"TestCsFiles/WorkspaceComponentRegistryTestFileThatCompiles.cs");
            var registry = new WorkspaceComponentRegistry(workspace);

            var components = (await registry.GetComponentsAsync()).ToList();

            CollectionAssert.AreEquivalent(new[] { "Component1", "Component2" }, components.Select(x => x.Name));
        }
    }
}
