using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    /// <summary>
    /// Finds the links for a given item
    /// </summary>
    public class LinksManager
    {
        public IEnumerable<ProjectItem> GetItemsToLinkTo(ProjectItem file)
        {
            return new[] { file };
        }
    }
}
