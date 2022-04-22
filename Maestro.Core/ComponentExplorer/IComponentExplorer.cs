using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.ComponentExplorer
{
    public interface IComponentExplorer
    {
        List<IComponentTag> Tags { get; }
    }
}
