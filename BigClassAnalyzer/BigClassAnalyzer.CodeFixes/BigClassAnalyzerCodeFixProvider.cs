using Maestro.Core.CodingConstructs.Classes;
using Maestro.Core.CodingConstructs.Classes.Graphs;
using Maestro.Core.CodingConstructs.Classes.Graphs.Nodes;
using Maestro.Core.CodingConstructs.Classes.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BigClassAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BigClassAnalyzerCodeFixProvider)), Shared]
    public class
        BigClassAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(BigClassAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent
                .AncestorsAndSelf()
                .OfType<ClassDeclarationSyntax>().
                First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedSolution: c => DecomposeClass(context.Document, root, classDeclaration, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private async Task<Solution> DecomposeClass(Document document,
            SyntaxNode syntaxTreeRoot,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken cancellationToken)
        {
            var result = document.Project.Solution;
            if (cancellationToken.IsCancellationRequested)
                return result;

            var rewriter = new Rewriter(classDeclaration);
            var updatedRoot = rewriter.Visit(syntaxTreeRoot);

            var updatedDocument = document.WithSyntaxRoot(updatedRoot);
            return updatedDocument.Project.Solution;
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private readonly ClassDeclarationSyntax _classToRemove;

            private readonly List<List<CSharpSyntaxNode>> _componentsNodes = new List<List<CSharpSyntaxNode>>();

            public Rewriter(ClassDeclarationSyntax classToRemove)
            {
                _classToRemove = classToRemove;

                var methodDeclarations = classToRemove.ChildNodes()
                .OfType<MethodDeclarationSyntax>()
                .ToDictionary(x => x.Identifier.ValueText, x => x);

                var fieldDeclarations = classToRemove.ChildNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .ToDictionary(x => x.Declaration.Variables.Single().Identifier.ValueText, x => x);

                var components = GetComponents(classToRemove);

                foreach (var component in components)
                {
                    var methods = component.OfType<MethodNode>().Select(x => methodDeclarations[x.Name]);
                    var variables = component.OfType<MethodNode>().Select(x => fieldDeclarations[x.Name]);

                    var syntaxNodes = variables.Cast<CSharpSyntaxNode>().Concat(methods).ToList();
                    _componentsNodes.Add(syntaxNodes);
                }
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node == _classToRemove)
                {
                    var newClass = SyntaxFactory.ClassDeclaration("Component1")
                        .WithMembers(new SyntaxList<MemberDeclarationSyntax>(_componentsNodes.First().OfType<MemberDeclarationSyntax>()));

                    return newClass;
                }
                else
                    return base.VisitClassDeclaration(node);
            }

            private List<List<Node>> GetComponents(SyntaxNode classDeclaration)
            {
                var factory = new CSharpClassParserFactory();
                var parser = factory.CreateParser(classDeclaration);

                var builder = new InternalClassGraphBuilder(parser);
                var graph = builder.Build();

                var analyzer = new InternalClassGraphAnalyzer();
                var components = analyzer.FindConnectedComponents(graph);

                return components;
            }
        }

        /*private void CreateClass(List<Node> nodes, SyntaxNode classDeclaration, string className)
        {
            var fieldNodes = nodes.OfType<VariableNode>().ToList();
            var methodNodes = nodes.OfType<MethodNode>().ToList();

            var fieldMatches = classDeclaration.ChildNodes().Where(x => x.IsKind(SyntaxKind.FieldDeclaration));
            var methodMatches = classDeclaration.ChildNodes().Where(x => x.IsKind(SyntaxKind.MethodDeclaration));

            
        }*/
    }
}
