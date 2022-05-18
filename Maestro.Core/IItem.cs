using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace Maestro.Core
{
    public class ProjectItem : IEquatable<ProjectItem>
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

        public static bool operator==(ProjectItem lhs, ProjectItem rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator!=(ProjectItem lhs, ProjectItem rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(ProjectItem other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return PathUtils.PointToTheSameFile(RelativeProjectFilePath, other.RelativeProjectFilePath)
                && PathUtils.PointToTheSameFile(RelativeItemFilePath, other.RelativeItemFilePath);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProjectItem);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                foreach (var piece in PathUtils.SplitPath(RelativeProjectFilePath).Concat(PathUtils.SplitPath(RelativeItemFilePath)))
                {
                    result += (piece ?? String.Empty).ToUpperInvariant().GetHashCode();
                }

                return result;
            }
        }
    }

    public static class PathUtils
    {
        public static bool PointToTheSameFile(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
                return true;

            var pieces1 = SplitPath(path1);
            var pieces2 = SplitPath(path2);

            return pieces1.SequenceEqual(pieces2, StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Regex _pathSep = new Regex(@"[/\\]", RegexOptions.Compiled);

        public static string[] SplitPath(string path)
        {
            return _pathSep.Split(path);
        }
    }
}
