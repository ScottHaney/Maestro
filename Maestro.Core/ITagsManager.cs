using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Abstractions;
using Newtonsoft.Json;

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

        public void Save(IFileSystem fileSystem, LinkFileContent contents)
        {
            var directory = Path.GetDirectoryName(FullPath);
            if (!fileSystem.Directory.Exists(directory))
                fileSystem.Directory.CreateDirectory(directory);

            fileSystem.File.WriteAllText(FullPath, JsonConvert.SerializeObject(contents));
        }
    }

    public class LinkFileContent
    {
        public readonly string LinkedFilePath;
        public readonly ProjectIdentifier ProjectIdentifier;

        public LinkFileContent(string linkedFilePath,
            ProjectIdentifier projectIdentifier)
        {
            LinkedFilePath = linkedFilePath;
            ProjectIdentifier = projectIdentifier;
        }
    }

    public class ProjectIdentifier
    {
        public readonly string ProjectPath;
        public readonly Guid Id;

        public ProjectIdentifier(string projectPath, Guid id)
        {
            ProjectPath = projectPath;
            Id = id;
        }
    }
}
