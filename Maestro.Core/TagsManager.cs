using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Maestro.Core
{
    public class TagsManager : ITagsManager
    {
        private readonly IFileSystem _fileSystem;

        private const string TAGS_FOLDER_NAME = "__Tags";
        private static readonly Regex _tagsFolderPathPartRegex = new Regex($@"[/\\]{TAGS_FOLDER_NAME}[/\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public TagsManager(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddItem(IProjectItem item, ITag tag)
        {
            var tagsFolder = GetTagsFolder(item.Project);
            var linkFile = tagsFolder.GetLinkFile(item, tag);

            var linkFileContents = new LinkFileContent(item.GetRelativeFilePath(), null);
            linkFile.Save(_fileSystem, linkFileContents);
        }

        public Tag GetTagFromCopiedToPath(string copiedToPath)
        {
            var tagName = Path.GetFileName(Path.GetDirectoryName(copiedToPath));
            if (string.Compare(tagName, TAGS_FOLDER_NAME, StringComparison.OrdinalIgnoreCase) == 0)
                tagName = "";

            return new Tag(tagName);
        }

        public void CreateLink(string targetProjectFile, string oldPath, string newPath)
        {
            var project = new Project(targetProjectFile);
            var projectItem = new ProjectItem(oldPath, project);

            var tagsManager = new TagsManager(new FileSystem());
            tagsManager.AddItem(projectItem, GetTagFromCopiedToPath(newPath));
        }

        public TagsFolder GetTagsFolder(IProject project)
        {
            return new TagsFolder(Path.Combine(project.FolderPath, TagsManager.TAGS_FOLDER_NAME));
        }

        public bool IsInTagsFolder(string filePath)
        {
            return _tagsFolderPathPartRegex.IsMatch(filePath);
        }
    }
}
