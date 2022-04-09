using ComponentsVSExtension.ToolWindows;
using ComponentsVSExtension.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ComponentsVSExtension
{
    public partial class MyToolWindowControl : UserControl
    {
        public MyToolWindowControl()
        {
            InitializeComponent();

            DataContext = new MyToolWindowViewModel();
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            var componentNames = await GetComponentNamesAsync();
            var viewModel = (MyToolWindowViewModel)DataContext;

            viewModel.Components = new System.Collections.ObjectModel.ObservableCollection<ComponentViewModel>(componentNames.Select(x => new ComponentViewModel() { Name = x }));
        }

        private async Task<IEnumerable<string>> GetComponentNamesAsync()
        {
            var syntaxTree = await VisualStudioWorkspaceUtils.GetActiveDocumentSyntaxTreeAsync();

            if (syntaxTree.TryGetRoot(out var root))
            {
                return root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(x => x.Identifier.ValueText);
            }

            return Enumerable.Empty<string>();
        }
    }
}