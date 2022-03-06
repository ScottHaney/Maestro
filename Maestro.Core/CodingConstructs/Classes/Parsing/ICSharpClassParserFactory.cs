using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Parsing
{
    public interface ICSharpClassParserFactory
    {
        ICSharpClassParser CreateParser(SyntaxNode classDefinition);

        ICSharpClassParser CreateParser(string csFileWithClass);
    }
}
