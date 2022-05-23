using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Core.Tests
{
    public class ProjectItemTests
    {
        [Test]
        public void Two_Identical_ProjectItems_Are_Equal()
        {
            var item1 = new ProjectItem(@"Project1\Project1.csproj", @"Project1\Folder\Class.cs");
            var item2 = new ProjectItem(@"Project1\Project1.csproj", @"Project1\Folder\Class.cs");

            Assert.IsTrue(item1 == item2);
        }
    }
}
