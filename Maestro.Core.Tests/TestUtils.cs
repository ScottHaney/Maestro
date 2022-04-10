using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Maestro.Core.Tests
{
    public static class TestUtils
    {
        public static SemanticModel GetSemanticModel(string relativeFilePath)
        {
            var filePath = Path.Combine(GetTestFilesBasePath(), relativeFilePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));

            var compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(syntaxTree);

            return compilation.GetSemanticModel(syntaxTree);
        }

        private static string GetTestFilesBasePath()
        {
            var prefixToRemove = @"file:\";
            
            var result = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (result.StartsWith(prefixToRemove, StringComparison.OrdinalIgnoreCase))
                result = result.Substring(prefixToRemove.Length);

            return result;
        }
    }
}
