using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Maestro.Core.CodingConstructs.Classes.Graphs;

namespace Maestro.Core.CodingConstructs.Classes.Parsing
{
    /// <summary>
    /// Retrieves the information from a C# class that will be needed to create an <see cref="IInternalClassGraph"/>
    /// </summary>
    public interface ICSharpClassParser
    {
        IEnumerable<string> GetVariableNames();

        IEnumerable<MethodReferences> GetMethodsInfo();
    }

    public class MethodReferences
    {
        public readonly string MethodName;
        public readonly IEnumerable<string> ReferencedVariableNames;
        public readonly IEnumerable<string> CalledMethodNames;

        public MethodReferences(string methodName,
            IEnumerable<string> referencedVariableNames,
            IEnumerable<string> calledMethodNames)
        {
            MethodName = methodName;
            ReferencedVariableNames = referencedVariableNames;
            CalledMethodNames = calledMethodNames;
        }
    }
}
