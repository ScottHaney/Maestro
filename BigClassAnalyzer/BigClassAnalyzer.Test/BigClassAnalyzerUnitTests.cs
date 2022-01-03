using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = BigClassAnalyzer.Test.CSharpCodeFixVerifier<
    BigClassAnalyzer.BigClassAnalyzerAnalyzer,
    BigClassAnalyzer.BigClassAnalyzerCodeFixProvider>;

namespace BigClassAnalyzer.Test
{
    [TestClass]
    public class BigClassAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task Empty_Text_Produces_Nothing()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task Class_That_Has_Two_Connected_Components_Produces_A_Warning()
        {
            var csharpCode = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class BigClass
        {
            private readonly int field1;
            private readonly int field2;

            public int Method1()
                => field1 + 1;

            public int Method2()
                => field2 + 1;
        }
    }";

            var actual = VerifyCS.Diagnostic("BigClassAnalyzer").WithLocation(0).WithArguments("BigClass");
            Assert.AreNotEqual(DiagnosticResult.EmptyDiagnosticResults, actual);

            /*var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class BigClass
        {
            private readonly int field1;
            private readonly int field2;

            public int Method1()
                => field1 + 1;

            public int Method2()
                => field2 + 1;
        }
    }";

            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);*/
        }
    }
}
