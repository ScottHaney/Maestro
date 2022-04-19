using Maestro.Core.Components;
using Maestro.VSExtension.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsVSExtension.Utils
{
    public static class VisualStudioWorkspaceUtils
    {
        public static async Task<SyntaxTree> GetActiveDocumentSyntaxTreeAsync()
        {
            var activeDocument = await GetActiveDocumentAsync();

            if (activeDocument != null)
                return await activeDocument.GetSyntaxTreeAsync();

            return null;
        }

        private static async Task<Document> GetActiveDocumentAsync()
        {
            var activeDocumentPath = await GetActiveDocumentPathAsync();

            if (activeDocumentPath != null)
            {
                var workspace = ComponentsVSExtensionPackage.CurrentWorkspace;
                var docIds = workspace.CurrentSolution.GetDocumentIdsWithFilePath(activeDocumentPath);

                if (docIds.Count() == 1)
                {
                    var docId = docIds.Single();
                    return workspace.CurrentSolution.GetDocument(docId);
                }
            }

            return null;
        }

        private static async Task<string> GetActiveDocumentPathAsync()
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            return docView?.FilePath;
        }

        public static async Task<SnapshotSpan?> GetCurrentSelectionAsync()
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            return docView?.TextView.Selection.SelectedSpans.FirstOrDefault();
        }
    }

    public class VSSelectionFinder : ISelectionFinder
    {
        public async Task<List<SelectionSpan>> GetSelectionsAsync()
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();

            return docView.TextView.Selection.SelectedSpans
                .Select(x => new SelectionSpan(docView.FilePath, GetLineNumber(x.Start, docView).Value, GetLineNumber(x.End, docView).Value))
                .ToList();
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