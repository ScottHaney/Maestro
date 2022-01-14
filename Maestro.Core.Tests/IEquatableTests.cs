﻿using Autofac.Extras.Moq;
using NUnit.Framework;
using System;
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

            RunGetHashCodeChecks(node1, node1IdenticalTwin, node2);
        }

        private void RunGetHashCodeChecks<T>(params T[] items)
            where T : IEquatable<T>
        {
            var set = new HashSet<T>();
            foreach (var item in items)
                set.Add(item);

            foreach (var item in items)
                Assert.IsTrue(set.Contains(item));
        }
    }
}
