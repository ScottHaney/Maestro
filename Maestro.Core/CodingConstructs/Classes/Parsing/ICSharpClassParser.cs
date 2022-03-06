using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Parsing
{
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
