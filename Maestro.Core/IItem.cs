using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Maestro.Core
{
    public class ProjectItem
    {
        [JsonProperty]
        private string RelativeProjectFilePath { get; set; }

        [JsonProperty]
        private string RelativeItemFilePath { get; set; }

        [JsonIgnore]
        public string FileName => Path.GetFileName(RelativeItemFilePath);

        public ProjectItem(string fullItemFilePath, string fullProjectFilePath, string fullSolutionFilePath)
        {
            RelativeProjectFilePath = string.IsNullOrEmpty(fullProjectFilePath) ? "" : PathNetCore.GetRelativePath(Path.GetDirectoryName(fullSolutionFilePath), fullProjectFilePath);
            RelativeItemFilePath = PathNetCore.GetRelativePath(Path.GetDirectoryName(fullSolutionFilePath), fullItemFilePath);
        }

        public ProjectItem(string relativeItemPath)
        {
            RelativeProjectFilePath = "";
            RelativeItemFilePath = relativeItemPath;
        }

        [JsonConstructor]
        public ProjectItem(string relativeProjectFilePath, string relativeItemFilePath)
        {
            RelativeProjectFilePath = relativeProjectFilePath;
            RelativeItemFilePath = relativeItemFilePath;
        }

        public LinkFileContent GetLinkFileContent()
            => new LinkFileContent(RelativeItemFilePath, new ProjectIdentifier(RelativeProjectFilePath, Guid.NewGuid()));

        public string GetFullItemPath(string solutionFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(solutionFilePath), RelativeItemFilePath);
        }

        public string GetFullProjectPath(string solutionFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(solutionFilePath), RelativeProjectFilePath);
        }

        public string GetRelativePathForGit(string solutionFilePath)
        {
            var solutionDir = Path.GetDirectoryName(solutionFilePath);
            return PathNetCore.GetRelativePath(solutionDir, GetFullItemPath(solutionFilePath));
        }

        public bool IsLinkFile()
        {
            return LinkFile.TryParse(RelativeItemFilePath, out var _);
        }

        public bool IsProjectOrSolutionFile()
        {
            var extension = Path.GetExtension(FileName);
            return string.Compare(extension, ".csproj", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(extension, ".sln", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
