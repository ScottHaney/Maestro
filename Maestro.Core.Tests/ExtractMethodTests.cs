using Autofac;
using Autofac.Extras.Moq;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.Tests
{
    public class ExtractMethodTests
    {
        [Test]
        public void Test()
        {
            var classText = @"public class Test { public readonly int Field1; public readonly int Field2; public int TestMethod1() { return Field1; } public int TestMethod2() { return Field2; } }";

            using (var mock = AutoMock.GetLoose(cb =>
            {

            }))
            {
                var tree = CSharpSyntaxTree.ParseText(classText);
            }
        }
    }
}
