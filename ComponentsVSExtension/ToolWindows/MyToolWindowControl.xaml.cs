using System.Linq;
using System.Text;
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

            var _selection = docView?.TextView.Selection;

            if (selection.HasValue)
            {
                var startLine = GetLineNumber(selection.Value.Start.Position, docView);
                var endLine = GetLineNumber(selection.Value.End.Position, docView);

                if (startLine.HasValue && endLine.HasValue)
                {
                    var minLine = Math.Min(startLine.Value, endLine.Value);
                    var maxLine = Math.Max(startLine.Value, endLine.Value);

                    var sb = new StringBuilder();
                    foreach (var line in docView.TextView.TextViewLines.Skip(minLine).Take(maxLine - minLine + 1))
                    {
                        sb.AppendLine(line.Extent.GetText());
                    }

                    VS.MessageBox.Show("Test", CreateTextToAdd(sb.ToString()));
                }
            }
        }

        private string CreateTextToAdd(string methodCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"public interface IComponent
{
    void Method();
}");

            sb.AppendLine();

            sb.AppendLine(@"public class Component : IComponent
{
    public void Method()
    {");
            sb.Append(methodCode);

            sb.AppendLine(@"    }
}");

            return sb.ToString();
        }

        private int? GetLineNumber(int position,
            DocumentView documentView)
        {
            var lines = documentView.TextView.TextViewLines;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                var start = line.Start.Position;
                var end = line.End.Position;
                if (position >= start && position <= end)
                    return i;
            }

            return null;
        }
    }
}