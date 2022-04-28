using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TagsVSExtension
{
    public static class ProjectUtils
    {
        private static readonly Regex _tagsFolderPathPartRegex = new Regex(@"[/\\]__Tags", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetProjectFilePath(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            while (directory != null)
            {
                var projFiles = Directory.EnumerateFiles(directory, "*.csproj");
                if (projFiles.Any())
                    return projFiles.First();

                directory = Path.GetDirectoryName(directory);
            }

            return string.Empty;
        }

        public static bool IsInToTagsFolder(string filePath)
        {
            return _tagsFolderPathPartRegex.IsMatch(filePath);
        }

        public static void CreateLink(string targetProjectFile, string oldPath, string newPath)
        {
            var project = new Maestro.Core.Project(targetProjectFile);
            var projectItem = new Maestro.Core.ProjectItem(oldPath, project);

            var tagName = Path.GetFileName(Path.GetDirectoryName(newPath));
            if (string.Compare(tagName, "__Tags", StringComparison.OrdinalIgnoreCase) == 0)
                tagName = "";

            var tagsManager = new Maestro.Core.TagsManager(new FileSystem());
            tagsManager.AddItem(projectItem, new Maestro.Core.Tag(tagName));
        }
    }
}
