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
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
        }
    }";

            var fixtest = @"
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

            var expected = VerifyCS.Diagnostic("BigClassAnalyzer").WithLocation(0).WithArguments("BigClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
