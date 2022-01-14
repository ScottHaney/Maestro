using Autofac.Extras.Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Maestro.Core.Tests
{
    public class IEquatableTests
    {
        [Test]
        public void InternalClassNode_Equality_Checks()
        {
            var node1 = new InternalClassNode("node", InternalClassNodeType.Function);
            var node1IdenticalTwin = new InternalClassNode("node", InternalClassNodeType.Function);
            var node2 = new InternalClassNode("node", InternalClassNodeType.Variable);

            Assert.IsTrue(node1.Equals(node1IdenticalTwin));
            Assert.IsTrue(node1.Equals((object)node1IdenticalTwin));
            Assert.IsTrue(node1 == node1IdenticalTwin);

            Assert.IsFalse(node1.Equals(node2));
            Assert.IsFalse(node1.Equals((object)node2));
            Assert.IsTrue(node1 != node2);

            //Do these tests to run a check on the GetHashCode implementation
            var set = new HashSet<InternalClassNode>() { node1, node1IdenticalTwin, node2 };
            Assert.IsTrue(set.Contains(node1));
            Assert.IsTrue(set.Contains(node1IdenticalTwin));
            Assert.IsTrue(set.Contains(node2));
        }
    }
}
