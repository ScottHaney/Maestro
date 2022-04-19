using Maestro.Core.Components;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maestro.VSExtension.ViewModels
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

        public ICommand DeleteComponent { get; set; }

        public ICommand MergeComponents { get; set; }

        public ICommand CreateComponents { get; set; }


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
        private readonly WorkspaceComponentRegistry _componentsRegistry;

        public UpdateCommand(MyToolWindowViewModel vm, Workspace currentWorkspace)
        {
            _vm = vm;
            _componentsRegistry = new WorkspaceComponentRegistry(currentWorkspace);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var components = await GetComponentsAsync();

            _vm.Components = new ObservableCollection<ComponentViewModel>(components.Select(x => new ComponentViewModel(x)));
        }

        private async Task<IEnumerable<ICodeComponent>> GetComponentsAsync()
        {
            return await _componentsRegistry.GetComponentsAsync();
        }
    }

    public class MergeComponentsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly MyToolWindowViewModel _vm;
        private readonly WorkspaceComponentRegistry _componentsRegistry;

        public MergeComponentsCommand(MyToolWindowViewModel vm, Workspace currentWorkspace)
        {
            _vm = vm;
            _componentsRegistry = new WorkspaceComponentRegistry(currentWorkspace);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var mergeComponentsParameter = (MergeComponentsParameter)parameter;

            var manager = new ComponentManager();
            await manager.MergeComponentsAsync(mergeComponentsParameter.Source, mergeComponentsParameter.Destination);

            var components = await _componentsRegistry.GetComponentsAsync();
            _vm.Components = new ObservableCollection<ComponentViewModel>(components.Select(x => new ComponentViewModel(x)));
        }

        private async Task<IEnumerable<ICodeComponent>> GetComponentsAsync()
        {
            return await _componentsRegistry.GetComponentsAsync();
        }
    }

    public class MergeComponentsParameter
    {
        public readonly ICodeComponent Source;
        public readonly ICodeComponent Destination;

        public MergeComponentsParameter(ICodeComponent source,
            ICodeComponent destination)
        {
            Source = source;
            Destination = destination;
        }
    }

    public class DeleteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly MyToolWindowViewModel _vm;
        private readonly WorkspaceComponentRegistry _componentsRegistry;

        public DeleteCommand(MyToolWindowViewModel vm, Workspace currentWorkspace)
        {
            _vm = vm;
            _componentsRegistry = new WorkspaceComponentRegistry(currentWorkspace);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var componentViewModel = (ComponentViewModel)parameter;

            var manager = new ComponentManager();
            await manager.DeleteComponentAsync(new CodeComponent(componentViewModel.Name, componentViewModel.Source));

            var components = await _componentsRegistry.GetComponentsAsync();
            _vm.Components = new ObservableCollection<ComponentViewModel>(components.Select(x => new ComponentViewModel(x)));
        }
    }

    public class CreateComponentsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly MyToolWindowViewModel _vm;
        private readonly WorkspaceComponentRegistry _componentsRegistry;
        private readonly ISelectionFinder _selectionFinder;
        private readonly Workspace _currentWorkspace;

        public CreateComponentsCommand(MyToolWindowViewModel vm, Workspace currentWorkspace, ISelectionFinder selectionFinder)
        {
            _vm = vm;
            _componentsRegistry = new WorkspaceComponentRegistry(currentWorkspace);
            _selectionFinder = selectionFinder;
            _currentWorkspace = currentWorkspace;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var selections = await _selectionFinder.GetSelectionsAsync();

            var manager = new ComponentManager();
            for (int i = 0; i < selections.Count; i++)
            {
                await manager.CreateComponentAsync(selections[i], _currentWorkspace, $"Component{i + 1}");
            }
        }
    }

    public interface ISelectionFinder
    {
        Task<List<SelectionSpan>> GetSelectionsAsync();
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

        public CodeComponentSourceId Source { get; set; }

        public ComponentViewModel(ICodeComponent component)
        {
            Name = component.Name;
            Source = component.SourceId;
        }

        public CodeComponent ToCodeComponent()
        {
            return new CodeComponent(Name, Source);
        }

        public ComponentViewModel()
        { }

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
