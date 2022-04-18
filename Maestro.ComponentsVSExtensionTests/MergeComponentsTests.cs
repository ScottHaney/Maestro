using NUnit.Framework;
using TestUtils;
using System.Linq;
using System.Threading.Tasks;
using Maestro.VSExtension.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Maestro.ComponentsVSExtensionTests
{
    public class MergeComponentsTests
    {
        [Test]
        public async Task Merge_Components()
        {
            var workspace = SingleTestDocumentWorkspace.Create(@"TestCsFiles/MergeComponentsTestFile.cs");

            var vm = new MyToolWindowViewModel();
            vm.MergeComponents = new MergeComponentsCommand(vm, workspace.GetWorkspace());

            var components = (await workspace.GetRegistry().GetComponentsAsync()).ToList();

            vm.Components = new System.Collections.ObjectModel.ObservableCollection<ComponentViewModel>(components.Select(x => new ComponentViewModel(x)));

            vm.MergeComponents.Execute(new MergeComponentsParameter(vm.Components.First(x => x.Name == "Component2").ToCodeComponent(),
                vm.Components.First(x => x.Name == "Component1").ToCodeComponent()));

            var updatedComponents = (await workspace.GetRegistry().GetComponentsAsync()).ToList();

            CollectionAssert.AreEquivalent(new[] { "Component1" }, updatedComponents.Select(x => x.Name));
            CollectionAssert.AreEquivalent(new[] { "Component1" }, vm.Components.Select(x => x.Name));

            var updatedSyntaxTree = workspace.GetSyntaxTree();
            var componentMatch = updatedSyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Single(x => x.Identifier.Text == "Component1");

            var properties = componentMatch
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .ToList();

            CollectionAssert.AreEquivalent(new[] { "Item1", "Item2" }, properties.Select(x => x.Identifier.Text));
        }
    }
}
