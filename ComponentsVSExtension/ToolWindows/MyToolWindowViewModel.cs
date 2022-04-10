using ComponentsVSExtension.Utils;
using Maestro.Core.Components;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            var components = await GetComponentsAsync();

            _vm.Components = new ObservableCollection<ComponentViewModel>(components.Select(x => new ComponentViewModel() { Name = x.Name }));
        }

        private async Task<IEnumerable<ICodeComponent>> GetComponentsAsync()
        {
            var syntaxTree = await VisualStudioWorkspaceUtils.GetActiveDocumentSyntaxTreeAsync();
            return RoslynHelpers.GetComponents(syntaxTree);
            
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

    public class MyToolWindowDesignViewModel
    {
        public List<ComponentViewModel> Components { get; set; }

        public MyToolWindowDesignViewModel()
        {
            Components = new List<ComponentViewModel>()
            {
                new ComponentViewModel() { Name = "Test1" },
                new ComponentViewModel() { Name = "Test2" }
            };
        }
    }
}
