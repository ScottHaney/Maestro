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

            var vm = new MyToolWindowViewModel();
            vm.Update = new UpdateCommand(vm);

            DataContext = vm;
        }
    }
}