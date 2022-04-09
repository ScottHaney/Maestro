using ComponentsVSExtension.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComponentsVSExtension.ToolWindows
{
    public class MyToolWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ComponentViewModel> _components;
        public ObservableCollection<ComponentViewModel> Components
        {
            get { return _components; }
            set
            {
                _components = value;
                OnPropertyChanged(nameof(Components));
            }
        }

        public ICommand Update { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }
    }

    public class UpdateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly MyToolWindowViewModel _vm;

        public UpdateCommand(MyToolWindowViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var componentNames = await GetComponentNamesAsync();

            _vm.Components = new ObservableCollection<ComponentViewModel>(componentNames.Select(x => new ComponentViewModel() { Name = x }));
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

    public class ComponentViewModel : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
