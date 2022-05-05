using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Maestro.Core
{
    public class SelectionManager
    {
        private readonly IVisualStudioSolution _vsSolution;

        public SelectionManager(IVisualStudioSolution vsSolution)
        {
            _vsSolution = vsSolution;
        }

        public async Task ItemsSelectedAsync(IList<ProjectItem> files)
        {
            if (files.Count == 1)
            {
                var selectedItem = files.Single();
                if (!selectedItem.IsLinkFile())
                {
                    var linkFilePath = await _vsSolution.CreateLinkFileAsync(selectedItem);
                    if (!_vsSolution.ProjectAlreadyHasLink(selectedItem, linkFilePath))
                        _vsSolution.AddProjectItem(selectedItem, linkFilePath);
                }
            }
        }

        public void ItemsDeselected(IList<ProjectItem> files)
        {
            if (files.Count == 1)
            {
                var file = files.Single();
                if (file != null)
                    _vsSolution.RemoveLinkFiles(file);
            }
        }
    }

    public interface IVisualStudioSolution
    {
        Task<string> CreateLinkFileAsync(ProjectItem projectItem);
        bool ProjectAlreadyHasLink(ProjectItem projectItem, string linkFilePath);
        void AddProjectItem(ProjectItem projectItem, string linkFilePath);
        void RemoveLinkFiles(ProjectItem projectItem);
    }
}
