using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Abstractions;

namespace Maestro.Core
{
    public class TagsManager : ITagsManager
    {
        private readonly IFileSystem _fileSystem;

        public TagsManager(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddItem(TagsFolderPath tagsFolder, IProjectItem item, ITag tag)
        {
            var linkFilePath = tagsFolder.GetFilePath(item, tag);
            var linkedFilePath = PathNetCore.GetRelativePath(item.Project.FolderPath, item.FilePath);

            var directory = Path.GetDirectoryName(linkFilePath);
            if (!_fileSystem.Directory.Exists(directory))
                _fileSystem.Directory.CreateDirectory(directory);

            _fileSystem.File.WriteAllText(linkFilePath, linkedFilePath);
        }
    }
}
