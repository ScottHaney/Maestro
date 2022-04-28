using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Maestro.Core
{
    internal interface ITagsManager
    {
        void AddItem(TagsFolderPath tagsFolderPath, IProjectItem item, ITag tag);
    }

    public class TagsFolderPath
    {
        public readonly string FullPath;

        public TagsFolderPath(string fullPath)
        {
            FullPath = fullPath;
        }

        public string GetFilePath(IItem item, ITag tag)
        {
            return Path.Combine(FullPath, tag.Name, Path.GetFileName(item.FilePath) + ".link");
        }
    }
}
