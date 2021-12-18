using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Maestro.Core
{
    public class BigClassHelper
    {
        public InternalClassDiagram CreateDiagram(string csFileWithClass)
        {
            var tree = CSharpSyntaxTree.ParseText(csFileWithClass);
            var root = tree.GetCompilationUnitRoot();

            return new InternalClassDiagram();
        }
    }

    public class InternalClassDiagram
    {
        public bool IsEmpty => true;
    }
}
