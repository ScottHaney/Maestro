using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ComponentsVSExtension
{
    public partial class MyToolWindowControl : UserControl
    {
        public MyToolWindowControl()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var selection = docView?.TextView.Selection.SelectedSpans.FirstOrDefault();

            VS.MessageBox.Show("ComponentsVSExtension", selection?.GetText() ?? String.Empty);
        }
    }
}