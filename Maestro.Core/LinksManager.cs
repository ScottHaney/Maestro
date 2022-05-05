using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class LinksManager
    {
        public IEnumerable<ProjectItem> GetItemsToLinkTo(ProjectItem file)
        {
            return new[] { file };
        }
    }
}
