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
        private readonly string _solutionFolderPath;

        private readonly string _storageFolderPath;

        public HowAreLinkedFilesStored(IFileSystem fileSystem,
            string solutionFolderPath)
        {
            _fileSystem = fileSystem;
            _solutionFolderPath = solutionFolderPath;

            _storageFolderPath = Path.Combine(_storageFolderPath, "__Tags");
        }

        public IEnumerable<string> StoreLinkFiles(ProjectItem parentItem, IEnumerable<ProjectItem> links)
        {
            if (!_fileSystem.Directory.Exists(_storageFolderPath))
                _fileSystem.Directory.CreateDirectory(_storageFolderPath);

            foreach (var link in links)
            {
                var linkFileContent = link.GetLinkFileContent();

                var linkFilePath = Path.Combine(_storageFolderPath, link.FileName + ".link");
                _fileSystem.File.WriteAllText(linkFilePath,
                    JsonConvert.SerializeObject(linkFileContent));

                yield return linkFilePath;
            }
        }

        public void DeleteLinksForItem(ProjectItem projectItem)
        {
            if (_fileSystem.Directory.Exists(_storageFolderPath))
                _fileSystem.Directory.Delete(_storageFolderPath, true);
        }
    }
}
