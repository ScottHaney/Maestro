using Maestro.Core.CodingConstructs.Classes.Architecture;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.CodingConstructs.Classes.Graphs
{
    public interface IInternalClassGraphBuilder
    {
        void AddNode(VariableNode node);
        void AddNode(MethodNode node);
        IInternalClassGraph Build();
    }
}
