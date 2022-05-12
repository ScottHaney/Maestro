using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core.Links
{
    public class WhenAreLinkFilesShown
    {
        public event EventHandler<List<ProjectItem>> ShowLinks;
        public event EventHandler<List<ProjectItem>> HideLinks;

        private readonly IVisualWorkspace _visualWorkspace;
        private readonly HowAreLinkedFilesStored _howAreLinkedFilesStored;

        public WhenAreLinkFilesShown(IVisualWorkspace visualWorkspace,
            HowAreLinkedFilesStored howAreLinkedFilesStored)
        {
            _visualWorkspace = visualWorkspace;
            _howAreLinkedFilesStored = howAreLinkedFilesStored;

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
            {
                ShowLinks?.Invoke(this, e);
            }
        }
    }

    public interface IVisualWorkspace
    {
        event EventHandler<List<ProjectItem>> ItemsSelected;
        event EventHandler<List<ProjectItem>> ItemsUnselected;

        void ShowLinks(ProjectItem projectItem, IEnumerable<StoredLinkFile> linkedItems);
        void HideLinks(IEnumerable<ProjectItem> projectItems);
        void ClearLinks(IEnumerable<ExistingLinkFile> existingLinks);
    }
}
