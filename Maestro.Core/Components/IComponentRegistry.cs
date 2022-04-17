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

        private readonly Dictionary<Guid, Document> _componentsSourceMap = new Dictionary<Guid, Document>();

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
                        _componentsSourceMap[component.SourceId.Id] = document;
                        results.Add(component);
                    }
                }
            }

            return results;
        }

        public Document GetDocument(CodeComponentSourceId sourceId)
        {
            return _componentsSourceMap[sourceId.Id];
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
            var document = component.SourceId.Registry.GetDocument(component.SourceId);

            var syntaxRoot = await document.GetSyntaxRootAsync();

            var declarationNode = syntaxRoot.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .First(x => x.Identifier.Text == component.Name);

            var updatedTree = syntaxRoot.RemoveNode(declarationNode, SyntaxRemoveOptions.KeepExteriorTrivia);

            var updatedSolution = document.Project.Solution.WithDocumentSyntaxRoot(document.Id, updatedTree);
            document.Project.Solution.Workspace.TryApplyChanges(updatedSolution);
        }

        public Task MergeComponentsAsync(ICodeComponent sourceComponent, ICodeComponent destinationComponent)
        {
            throw new NotImplementedException();
        }
    }
}
