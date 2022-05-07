using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.Links
{
    public class WhichItemsShouldBeLinked
    {
        public IEnumerable<ProjectItem> GetLinks(ProjectItem projectItem)
        {
            return new[] { projectItem };
        }
    }
}
