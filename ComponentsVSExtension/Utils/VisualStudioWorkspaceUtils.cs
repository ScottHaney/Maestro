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
}
