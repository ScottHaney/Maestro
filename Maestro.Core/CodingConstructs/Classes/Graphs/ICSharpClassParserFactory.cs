using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public interface ICSharpClassParserFactory
    {
        ICSharpClassParser CreateParser(SyntaxNode node);

        ICSharpClassParser CreateParser(string csFileWithClass);
    }
}
