using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public class CSharpClassParserFactory : ICSharpClassParserFactory
    {
        public ICSharpClassParser CreateParser(SyntaxNode classDeclaration)
        {
            return new RoslynCSharpClassParser(classDeclaration);
        }

        public ICSharpClassParser CreateParser(string csFileWithClass)
        {
            var tree = CSharpSyntaxTree.ParseText(csFileWithClass);
            var root = tree.GetCompilationUnitRoot();

            var classDeclaration = GetClassDeclaration(root);
            if (classDeclaration == null)
                throw new Exception($"Could not find a class declaration in \"{nameof(csFileWithClass)}\"");

            return CreateParser(root);
        }

        private ClassDeclarationSyntax GetClassDeclaration(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax classDecl)
                return classDecl;
            else
                return node.ChildNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }
    }
}
