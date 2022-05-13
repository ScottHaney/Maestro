using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace Maestro.Core.Links
{
    public class HowToShowLinkFiles
    {
        private readonly IVisualWorkspace _visualWorkspace;
        private readonly Workspace _workspace;
        private readonly IFileSystem _fileSystem;

        public HowToShowLinkFiles(IVisualWorkspace visualWorkspace,
            Workspace workspace,
            IFileSystem fileSystem)
        {
            _visualWorkspace = visualWorkspace;
            _workspace = workspace;
            _fileSystem = fileSystem;

        }

        public void ShowLinks(ProjectItem projectItem, IEnumerable<StoredLinkFile> linkedItems)
        {
            HideLinksForPreviousSelection();

            _visualWorkspace.ShowLinks(projectItem, linkedItems);

            SaveSelectionHistory(projectItem);
        }

        public void HideLinks(IEnumerable<ProjectItem> projectItems)
        {
        }

        private void SaveSelectionHistory(ProjectItem projectItem)
        {
            var historyFilePath = GetHistoryFilePath();
            if (!_fileSystem.File.Exists(historyFilePath))
                _fileSystem.File.Create(historyFilePath).Close();

            _fileSystem.File.WriteAllText(historyFilePath, JsonConvert.SerializeObject(projectItem));
        }

        private void HideLinksForPreviousSelection()
        {
            var historyFilePath = GetHistoryFilePath();
            if (_fileSystem.File.Exists(historyFilePath))
            {
                var text = _fileSystem.File.ReadAllText(historyFilePath);
                if (!string.IsNullOrEmpty(text))
                {
                    var previousItem = JsonConvert.DeserializeObject<ProjectItem>(text);

                    _visualWorkspace.HideLinks(new [] {previousItem});

                    _fileSystem.File.WriteAllText(historyFilePath, "");
                }
            }
        }

        private string GetHistoryFilePath()
        {
            return Path.Combine(Path.GetDirectoryName(_workspace.CurrentSolution.FilePath), "maestrohistory.txt");
        }
    }
}
