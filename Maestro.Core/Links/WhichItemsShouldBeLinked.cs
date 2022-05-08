using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Maestro.Core.Links
{
    public class WhichItemsShouldBeLinked
    {
        private readonly Workspace _workspace;

        public WhichItemsShouldBeLinked(Workspace workspace)
        {
            _workspace = workspace;
        }

        public IEnumerable<ProjectItem> GetLinks(ProjectItem projectItem)
        {
            return new[] { projectItem };
        }
    }
}
