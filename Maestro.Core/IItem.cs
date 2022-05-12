using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Maestro.Core
{
    public class ProjectItem
    {
        private readonly string _relativeProjectFilePath;
        private readonly string _relativeItemFilePath;

        public string FileName => Path.GetFileName(_relativeItemFilePath);

        public ProjectItem(string fullItemFilePath, string fullProjectFilePath, string fullSolutionFilePath)
        {
            _relativeProjectFilePath = string.IsNullOrEmpty(fullProjectFilePath) ? "" : PathNetCore.GetRelativePath(Path.GetDirectoryName(fullSolutionFilePath), fullProjectFilePath);
            _relativeItemFilePath = PathNetCore.GetRelativePath(Path.GetDirectoryName(fullSolutionFilePath), fullItemFilePath);
        }

        public ProjectItem(string relativeItemPath)
        {
            _relativeProjectFilePath = "";
            _relativeItemFilePath = relativeItemPath;
        }

        public LinkFileContent GetLinkFileContent()
            => new LinkFileContent(_relativeItemFilePath, new ProjectIdentifier(_relativeProjectFilePath, Guid.NewGuid()));

        public string GetFullItemPath(string solutionFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(solutionFilePath), _relativeItemFilePath);
        }

        public string GetFullProjectPath(string solutionFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(solutionFilePath), _relativeProjectFilePath);
        }

        public string GetRelativePathForGit(string solutionFilePath)
        {
            var solutionDir = Path.GetDirectoryName(solutionFilePath);
            return PathNetCore.GetRelativePath(solutionDir, GetFullItemPath(solutionFilePath));
        }

        public bool IsLinkFile()
        {
            return LinkFile.TryParse(_relativeItemFilePath, out var _);
        }

        public bool IsProjectOrSolutionFile()
        {
            var extension = Path.GetExtension(FileName);
            return string.Compare(extension, ".csproj", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(extension, ".sln", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
