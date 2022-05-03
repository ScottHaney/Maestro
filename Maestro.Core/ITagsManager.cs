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
        void AddItem(ProjectItem item, ITag tag);
    }

    public class TagsFolder
    {
        public readonly string FullPath;

        public TagsFolder(string fullPath)
        {
            FullPath = fullPath;
        }

        public LinkFile GetLinkFile(ProjectItem item, ITag tag)
        {
            var path = Path.Combine(FullPath, tag.Name, item.FileName + ".link");
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

        public static bool TryParse(string filePath, out LinkFile linkFile)
        {
            if (string.Compare(Path.GetExtension(filePath), ".link", StringComparison.OrdinalIgnoreCase) == 0)
            {
                linkFile = new LinkFile(filePath);
                return true;
            }
            else
            {
                linkFile = null;
                return false;
            }

        }
    }

    public class LinkFileContent
    {
        public readonly string RelativeFilePath;
        public readonly ProjectIdentifier ProjectIdentifier;

        public LinkFileContent(string relativeFilePath, ProjectIdentifier projectIdentifier)
        {
            RelativeFilePath = relativeFilePath;
            ProjectIdentifier = projectIdentifier;
        }

        public string GetFullFilePath(string solutionFilePath)
        {
            return Path.Combine(
                Path.GetDirectoryName(solutionFilePath),
                Path.GetDirectoryName(ProjectIdentifier.RelativeProjectFilePath),
                RelativeFilePath);
        }
    }

    public class ProjectIdentifier
    {
        public readonly string RelativeProjectFilePath;
        public readonly Guid Id;

        public ProjectIdentifier(string relativeProjectFilePath, Guid id)
        {
            RelativeProjectFilePath = relativeProjectFilePath;
            Id = id;
        }
    }
}
