using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace TagsVSExtension
{
    public partial class TagsDetailWindowControl : UserControl
    {
        public TagsDetailWindowControl()
        {
            this.InitializeComponent();

            ComponentsVSExtensionPackage.SelectionEvents.SelectionChanged += SelectionEvents_SelectionChanged;
        }

        private void SelectionEvents_SelectionChanged(object sender, Community.VisualStudio.Toolkit.SelectionChangedEventArgs e)
        {
            var newSelection = e.To;

            DataContext = new 
        }
    }

    public class TagDetailsViewModel()
}