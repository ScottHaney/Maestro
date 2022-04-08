using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.Components
{
    public interface IComponent
    {
        string Name { get; }
    }

    public interface IComponentRegistry
    {
    }

    public interface IComponentRegistrySource
    {
        IEnumerable<IComponent> GetComponents();
    }
}
