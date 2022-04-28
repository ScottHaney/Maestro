using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public interface ITag
    {
        string Name { get; }
    }

    public class Tag : ITag
    {
        public string Name { get; private set; }

        public Tag(string name)
        {
            Name = name;
        }
    }
}
