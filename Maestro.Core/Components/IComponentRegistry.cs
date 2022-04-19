using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Core.Components
{
    public interface ICodeComponent
    {
        string Name { get; }

        CodeComponentSourceId SourceId { get; }
    }

    public class CodeComponentSourceId
    {
        public readonly IComponentRegistry Registry;
        public readonly Guid Id;

        public CodeComponentSourceId(IComponentRegistry registry,
            Guid id)
        {
            Registry = registry;
            Id = id;
        }
    }

    public class CodeComponent : ICodeComponent
    {
        public string Name { get; private set; }
        public CodeComponentSourceId SourceId { get; private set; }

        public CodeComponent(string name,
            CodeComponentSourceId sourceId)
        {
            Name = name;
            SourceId = sourceId;
        }
    }

    public interface IComponentRegistry
    {
        Task<IEnumerable<ICodeComponent>> GetComponentsAsync();
        Document GetDocument(CodeComponentSourceId sourceId);
    }

    public interface IComponentRegistrySource
    {
        IEnumerable<ICodeComponent> GetComponents();
    }

    public static class ExtractMethodHelper
    {
        public static void ExtractMethod(int startLine, int endLine, SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.SyntaxTree;
        }
    }

    public class WorkspaceComponentRegistry : IComponentRegistry
    {
        private readonly Workspace _workspace;

        private readonly Dictionary<Guid, DocumentId> _componentsSourceMap = new Dictionary<Guid, DocumentId>();

        public WorkspaceComponentRegistry(Workspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<IEnumerable<ICodeComponent>> GetComponentsAsync()
        {
            var results = new List<ICodeComponent>();
            foreach (var project in _workspace.CurrentSolution.Projects)
            {
                foreach (var document in project.Documents.Where(x => x.SupportsSyntaxTree))
                {
                    var syntaxTree = await document.GetSyntaxTreeAsync();
                    foreach (var component in GetComponents(syntaxTree))
                    {
                        _componentsSourceMap[component.SourceId.Id] = document.Id;
                        results.Add(component);
                    }
                }
            }

            return results;
        }

        public Document GetDocument(CodeComponentSourceId sourceId)
        {
            return _workspace.CurrentSolution.GetDocument(_componentsSourceMap[sourceId.Id]);
        }

        private IEnumerable<ICodeComponent> GetComponents(SyntaxTree syntaxTree)
        {
            if (syntaxTree.TryGetRoot(out var root))
            {
                return root.DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .Select(x => new CodeComponent(x.Identifier.ValueText, new CodeComponentSourceId(this, Guid.NewGuid())));
            }

            return Enumerable.Empty<ICodeComponent>();
        }
    }

    public interface IComponentManager
    {

    }

    public class ComponentManager : IComponentManager
    {
        public async Task DeleteComponentAsync(ICodeComponent component)
        {
            var document = GetDocument(component);

            var syntaxRoot = await document.GetSyntaxRootAsync();

            var declarationNode = GetDeclarationNode(syntaxRoot, component);

            var updatedTree = syntaxRoot.RemoveNode(declarationNode, SyntaxRemoveOptions.KeepExteriorTrivia);

            var updatedSolution = document.Project.Solution.WithDocumentSyntaxRoot(document.Id, updatedTree);
            document.Project.Solution.Workspace.TryApplyChanges(updatedSolution);
        }

        public async Task MergeComponentsAsync(ICodeComponent sourceComponent, ICodeComponent destinationComponent)
        {
            var sourceDocument = GetDocument(sourceComponent);

            var sourceSyntaxRoot = await sourceDocument.GetSyntaxRootAsync();

            var sourceDeclaration = GetDeclarationNode(sourceSyntaxRoot, sourceComponent);

            var updatedSourceTree = sourceSyntaxRoot.RemoveNode(sourceDeclaration, SyntaxRemoveOptions.KeepExteriorTrivia);

            var updatedSolution = sourceDocument.Project.Solution.WithDocumentSyntaxRoot(sourceDocument.Id, updatedSourceTree);
            var firstSucceeded = sourceDocument.Project.Solution.Workspace.TryApplyChanges(updatedSolution);

            var destinationDocument = GetDocument(destinationComponent);

            var destinationSyntaxRoot = await destinationDocument.GetSyntaxRootAsync();

            var destinationDeclaration = GetDeclarationNode(destinationSyntaxRoot, destinationComponent);

            var updatedDestinationRoot = destinationSyntaxRoot.InsertNodesBefore(destinationDeclaration.ChildNodes().First(), sourceDeclaration.ChildNodes());

            updatedSolution = destinationDocument.Project.Solution.WithDocumentSyntaxRoot(destinationDocument.Id, updatedDestinationRoot);
            var secondSucceeded = destinationDocument.Project.Solution.Workspace.TryApplyChanges(updatedSolution);
        }

        public async Task CreateComponentAsync(SelectionSpan selectionSpan, Workspace workspace, string componentName)
        {
            var document = FindDocument(selectionSpan.DocumentFilePath, workspace);

            var syntaxTree = await document.GetSyntaxTreeAsync();

            var sourceText = syntaxTree.GetText();
            var root = syntaxTree.GetRoot();

            var lineMappings = sourceText.Lines;

            var nodesToMove = new List<SyntaxNode>();
            for (int i = selectionSpan.StartLineIndex; i <= selectionSpan.EndLineIndex; i++)
            {
                nodesToMove.Add(root.FindNode(lineMappings[i].Span));
            }

            throw new NotImplementedException();
        }

        private Document FindDocument(string filePath, Workspace workspace)
        {
            foreach (var document in workspace.CurrentSolution.Projects.SelectMany(x => x.Documents))
            {
                if (string.Compare(document.FilePath, filePath, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return document;
            }

            return null;
        }

        private TypeDeclarationSyntax GetDeclarationNode(SyntaxNode root, ICodeComponent component)
        {
            return root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .First(x => x.Identifier.Text == component.Name);
        }

        private Document GetDocument(ICodeComponent component)
        {
            return component.SourceId.Registry.GetDocument(component.SourceId);
        }
    }

    public class SelectionSpan
    {
        public readonly string DocumentFilePath;
        public readonly int StartLineIndex;
        public readonly int EndLineIndex;

        public SelectionSpan(string documentFilePath, int startLineIndex, int endLineIndex)
        {
            DocumentFilePath = documentFilePath;
            StartLineIndex = startLineIndex;
            EndLineIndex = endLineIndex;
        }
    }
}
