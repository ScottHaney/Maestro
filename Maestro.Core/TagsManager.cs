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

        private readonly string _solutionFilePath;

        public TagsManager(IFileSystem fileSystem, string solutionFilePath)
        {
            _fileSystem = fileSystem;
            _solutionFilePath = solutionFilePath;
        }

        public void AddItem(ProjectItem item, ITag tag)
        {
            var tagsFolder = GetTagsFolder(item);
            var linkFile = tagsFolder.GetLinkFile(item, tag);

            var linkFileContents = item.GetLinkFileContent();
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
            var projectItem = new ProjectItem(oldPath, targetProjectFile, _solutionFilePath);
            AddItem(projectItem, GetTagFromCopiedToPath(newPath));
        }

        public TagsFolder GetTagsFolder(ProjectItem project)
        {
            var tagsFolderPath = Path.Combine(Path.GetDirectoryName(project.GetFullProjectPath(_solutionFilePath)), TAGS_FOLDER_NAME);
            return new TagsFolder(tagsFolderPath);
        }

        public bool IsInTagsFolder(string filePath)
        {
            return _tagsFolderPathPartRegex.IsMatch(filePath);
        }
    }
}
