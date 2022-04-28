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

        public void AddItem(IProjectItem item, ITag tag)
        {
            var linkFile = item.Project.GetTagsFolder().GetLinkFile(item, tag);
            linkFile.Save(_fileSystem, item.GetRelativeFilePath());
        }
    }
}
