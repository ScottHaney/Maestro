using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.Components
{
    public interface ICodeComponent
    {
        string Name { get; }
    }

    public class CodeComponent : ICodeComponent
    {
        public string Name { get; private set; }

        public CodeComponent(string name)
        {
            Name = name;
        }
    }

    public interface IComponentRegistry
    {
    }

    public interface IComponentRegistrySource
    {
        IEnumerable<ICodeComponent> GetComponents();
    }
}
