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
        }

        private async Task<IEnumerable<string>> GetComponentNamesAsync()
        {
            var activeDocumentPath = await GetActiveDocumentPath();

            if (activeDocumentPath != null)
            {
                var workspace = ComponentsVSExtensionPackage.CurrentWorkspace;
                var docIds = workspace.CurrentSolution.GetDocumentIdsWithFilePath(activeDocumentPath);

                if (docIds.Count() == 1)
                {
                    var docId = docIds.Single();
                    var document = workspace.CurrentSolution.GetDocument(docId);
                    var syntaxTree = await document.GetSyntaxTreeAsync();

                    if (syntaxTree.TryGetRoot(out var root))
                    {
                        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                        return classDeclarations.Select(x => x.Identifier.ValueText);
                    }
                }
            }

            return Enumerable.Empty<string>();
        }

        private async Task<string> GetActiveDocumentPath()
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            return docView?.FilePath;
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            var componentNames = await GetComponentNamesAsync();
            //MessageBox.Show("Component Names", string.Join(", ", componentNames));

            /*var docView = await VS.Documents.GetActiveDocumentViewAsync();
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

                    var lastPosition = docView.TextView.TextViewLines.Last().End.Position;
                    docView.TextBuffer.Insert(lastPosition - 1, CreateTextToAdd(sb.ToString()));
                }
            }*/
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