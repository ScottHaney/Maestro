using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Linq;

namespace Maestro.Core.Links
{
    public class HowAreLinkedFilesStored
    {
        private readonly IFileSystem _fileSystem;
        private readonly Workspace _workspace;

        public HowAreLinkedFilesStored(IFileSystem fileSystem,
            Workspace workspace)
        {
            _fileSystem = fileSystem;
            _workspace = workspace;
        }

        private string GetStorageFolderPath(ProjectItem parentItem)
        {
            return Path.GetDirectoryName(parentItem.GetFullItemPath(_workspace.CurrentSolution.FilePath));
        }

        public IEnumerable<StoredLinkFile> StoreLinkFiles(ProjectItem parentItem, IEnumerable<ProjectItem> links)
        {
            var storageFolderPath = GetStorageFolderPath(parentItem);

            foreach (var link in links)
            {
                var linkFileContent = link.GetLinkFileContent();

                var linkFilePath = Path.Combine(storageFolderPath, link.FileName + ".link");
                _fileSystem.File.WriteAllText(linkFilePath,
                    JsonConvert.SerializeObject(linkFileContent));

                yield return new StoredLinkFile(link, linkFilePath);
            }
        }

        public void DeleteLinksForItems(IEnumerable<ProjectItem> projectItems)
        {
            foreach (var projectItem in projectItems)
            {
                var storageFolderPath = GetStorageFolderPath(projectItem);
                if (_fileSystem.Directory.Exists(storageFolderPath))
                {
                    foreach (var linkFile in _fileSystem.Directory.GetFiles(storageFolderPath, "*.link"))
                    {
                        _fileSystem.File.Delete(linkFile);
                    }
                }
            }
        }
    }

    public class StoredLinkFile
    {
        public readonly ProjectItem ProjectItem;
        public readonly string LinkFilePath;

        public StoredLinkFile(ProjectItem projectItem,
            string linkFilePath)
        {
            ProjectItem = projectItem;
            LinkFilePath = linkFilePath;
        }
    }

    public class ExistingLinkFile
    {
        public readonly string FilePath;
        public readonly LinkFileContent Content;

        public ExistingLinkFile(string filePath,
            LinkFileContent content)
        {
            FilePath = filePath;
            Content = content;
        }
    }
}
