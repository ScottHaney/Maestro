using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.Links
{
    public class HowToShowLinkFiles
    {
        private readonly IVisualWorkspace _visualWorkspace;

        public HowToShowLinkFiles(IVisualWorkspace visualWorkspace)
        {
            _visualWorkspace = visualWorkspace;
        }

        public void ShowLinks(ProjectItem projectItem, IEnumerable<StoredLinkFile> linkedItems)
        {
            _visualWorkspace.ShowLinks(projectItem, linkedItems);
        }

        public void HideLinks(ProjectItem projectItem)
        {

        }
    }
}
