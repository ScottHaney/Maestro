using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.Links
{
    public class WhenAreLinkFilesShown
    {
        public event EventHandler<List<ProjectItem>> ShowLinks;
        public event EventHandler<List<ProjectItem>> HideLinks;

        public WhenAreLinkFilesShown(IVisualWorkspace visualWorkspace)
        {
            visualWorkspace.ItemsSelected += VisualWorkspace_ItemsSelected;
            visualWorkspace.ItemsUnselected += VisualWorkspace_ItemsUnselected;
        }

        private void VisualWorkspace_ItemsUnselected(object sender, List<ProjectItem> e)
        {
            HideLinks?.Invoke(this, e);
        }

        private void VisualWorkspace_ItemsSelected(object sender, List<ProjectItem> e)
        {
            if (e.Count == 1)
                ShowLinks?.Invoke(this, e);
        }
    }

    public interface IVisualWorkspace
    {
        event EventHandler<List<ProjectItem>> ItemsSelected;
        event EventHandler<List<ProjectItem>> ItemsUnselected;

        void ShowLinks(ProjectItem projectItem, IEnumerable<StoredLinkFile> linkedItems);
        void HideLinks(IEnumerable<ProjectItem> projectItems);
    }
}
