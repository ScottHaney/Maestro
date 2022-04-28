using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Abstractions;

namespace Maestro.Core
{
    internal interface ITagsManager
    {
        void AddItem(IProjectItem item, ITag tag);
    }

    public class TagsFolder
    {
        public readonly string FullPath;

        public TagsFolder(string fullPath)
        {
            FullPath = fullPath;
        }

        public LinkFile GetLinkFile(IItem item, ITag tag)
        {
            var path = Path.Combine(FullPath, tag.Name, Path.GetFileName(item.FilePath) + ".link");
            return new LinkFile(path);
        }
    }

    public class LinkFile
    {
        public readonly string FullPath;

        public LinkFile(string fullPath)
        {
            FullPath = fullPath;
        }

        public void Save(IFileSystem fileSystem, string contents)
        {
            var directory = Path.GetDirectoryName(FullPath);
            if (!fileSystem.Directory.Exists(directory))
                fileSystem.Directory.CreateDirectory(directory);

            fileSystem.File.WriteAllText(FullPath, contents);
        }
    }
}
