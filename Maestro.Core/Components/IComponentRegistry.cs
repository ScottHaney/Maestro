using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public static class RoslynHelpers
    {
        public static IEnumerable<ICodeComponent> GetComponents(SyntaxTree syntaxTree)
        {
            if (syntaxTree.TryGetRoot(out var root))
            {
                return root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(x => new CodeComponent(x.Identifier.ValueText));
            }

            return Enumerable.Empty<ICodeComponent>();
        }
    }
}
