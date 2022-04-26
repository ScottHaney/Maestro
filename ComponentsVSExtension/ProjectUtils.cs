using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ComponentsVSExtension
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
    }
}
