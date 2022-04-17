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
    }

    public class CodeComponent : ICodeComponent
    {
        public string Name { get; private set; }

        public CodeComponent(string name)
        {
            Name = name;
        }
    }

    public interface IComponentRegistry
    {
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
                    results.AddRange(RoslynHelpers.GetComponents(syntaxTree));
                }
            }

            return results;
        }

        private IEnumerable<ICodeComponent> GetComponents(SyntaxTree syntaxTree)
        {
            return RoslynHelpers.GetComponents(syntaxTree);
        }
    }

    public static class RoslynHelpers
    {
        public static IEnumerable<ICodeComponent> GetComponents(SyntaxTree syntaxTree)
        {
            if (syntaxTree.TryGetRoot(out var root))
            {
                return root.DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .Select(x => new CodeComponent(x.Identifier.ValueText));
            }

            return Enumerable.Empty<ICodeComponent>();
        }
    }
}
