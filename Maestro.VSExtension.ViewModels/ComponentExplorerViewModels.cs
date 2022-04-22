using Maestro.Core.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace Maestro.VSExtension.ViewModels
{
    public interface IComponentItemViewModel
    {

    }

    public class ComponentsFolderViewModel : IComponentItemViewModel
    {
        public string Name { get; set; }

        public ObservableCollection<ComponentsFolderItemViewModel> Items { get; set; } = new ObservableCollection<ComponentsFolderItemViewModel>();
    }

    public class ComponentsFolderItemViewModel : IComponentItemViewModel
    {
        public string Name { get; set; }
    }
}
