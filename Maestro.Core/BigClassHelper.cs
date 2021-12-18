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

            throw new NotImplementedException();
        }
    }

    public class InternalClassDiagram
    {

    }
}
