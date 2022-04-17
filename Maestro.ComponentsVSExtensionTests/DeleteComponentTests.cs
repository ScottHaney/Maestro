using NUnit.Framework;
using TestUtils;
using System.Linq;
using System.Threading.Tasks;
using Maestro.VSExtension.ViewModels;

namespace Maestro.ComponentsVSExtensionTests
{
    public class DeleteComponentTests
    {
        [Test]
        public async Task Deletes_A_Component()
        {
            var workspace = SingleTestDocumentWorkspace.Create(@"TestCsFiles/DeleteComponentTestFile.cs");

            var vm = new MyToolWindowViewModel();
            vm.DeleteComponent = new DeleteCommand(vm, workspace.GetWorkspace());

            var components = (await workspace.GetRegistry().GetComponentsAsync()).ToList();

            vm.Components = new System.Collections.ObjectModel.ObservableCollection<ComponentViewModel>(components.Select(x => new ComponentViewModel(x)));

            vm.DeleteComponent.Execute(vm.Components.First(x => x.Name == "Component1"));

            var updatedComponents = (await workspace.GetRegistry().GetComponentsAsync()).ToList();

            CollectionAssert.AreEquivalent(new[] { "Component2" }, updatedComponents.Select(x => x.Name));
            CollectionAssert.AreEquivalent(new[] { "Component2" }, vm.Components.Select(x => x.Name));
        }
    }
}