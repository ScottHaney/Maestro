using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.ComponentExplorer
{
    public interface IComponentTag
    {
        string Name { get; }
        List<IComponent> Components { get; }
    }

    public interface IComponent
    {
        string Name { get; }
    }
}
