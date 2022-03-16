using Autofac;
using Maestro.Core;
using Maestro.Core.CodingConstructs.Classes;
using Maestro.Core.CodingConstructs.Classes.Graphs;
using Maestro.Core.CodingConstructs.Classes.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace BigClassAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BigClassAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BigClassAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var factory = new CSharpClassParserFactory();
            var parser = factory.CreateParser(context.Node);

            var builder = new InternalClassGraphBuilder(parser);
            var graph = builder.Build();

            var analyzer = new InternalClassGraphAnalyzer();
            var components = analyzer.FindConnectedComponents(graph);

            if (components.Count > 1)
            {
                //Underline only the class name rather than the entire class to make it look cleaner
                var className = context.Node.ChildTokens().First(x => x.IsKind(SyntaxKind.IdentifierToken));

                var diagnostic = Diagnostic.Create(Rule, className.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
