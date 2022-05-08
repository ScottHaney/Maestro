using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;

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

        private string GetStorageFolderPath()
        {
            return Path.Combine(Path.GetDirectoryName(_workspace.CurrentSolution.FilePath), "__Tags");
        }

        public IEnumerable<StoredLinkFile> StoreLinkFiles(ProjectItem parentItem, IEnumerable<ProjectItem> links)
        {
            var storageFolderPath = GetStorageFolderPath();
            if (!_fileSystem.Directory.Exists(storageFolderPath))
                _fileSystem.Directory.CreateDirectory(storageFolderPath);

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
            var storageFolderPath = GetStorageFolderPath();
            if (_fileSystem.Directory.Exists(storageFolderPath))
                _fileSystem.Directory.Delete(storageFolderPath, true);
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
}
