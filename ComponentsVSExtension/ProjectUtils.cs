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
    }
}
