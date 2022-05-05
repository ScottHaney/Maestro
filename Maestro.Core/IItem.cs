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
            _relativeProjectFilePath = PathNetCore.GetRelativePath(Path.GetDirectoryName(fullSolutionFilePath), fullProjectFilePath);
            _relativeItemFilePath = PathNetCore.GetRelativePath(Path.GetDirectoryName(fullProjectFilePath), fullItemFilePath);
        }

        public LinkFileContent GetLinkFileContent()
            => new LinkFileContent(_relativeItemFilePath, new ProjectIdentifier(_relativeProjectFilePath, Guid.NewGuid()));

        public string GetFullItemPath(string solutionFilePath)
        {
            return Path.Combine(GetFullProjectPath(solutionFilePath), _relativeItemFilePath);
        }

        public string GetFullProjectPath(string solutionFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(solutionFilePath), _relativeProjectFilePath);
        }

        public bool IsLinkFile()
        {
            return LinkFile.TryParse(_relativeItemFilePath, out var _);
        }
    }
}
