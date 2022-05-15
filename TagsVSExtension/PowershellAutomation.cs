using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;

namespace TagsVSExtension
{
    public static class PowershellAutomation
    {
        public static IEnumerable<string> GetHistoryFromGit(string solutionFilePath, string filePath)
        {
            var ps = PowerShell.Create();
            string script = ChangedFilesHistory(filePath, solutionFilePath);
            ps.AddScript(script);
            ps.Runspace.SessionStateProxy.Path.SetLocation(Path.GetDirectoryName(solutionFilePath));

            var settings = new PSInvocationSettings();
            var results = ps.Invoke();

            return results.Select(x => x.ToString())
                .GroupBy(x => x)
                .OrderBy(x => x.Count())
                .Select(x => x.Key)
                .Where(x => !IsProjectOrSolutionFile(x))
                .Where(x => File.Exists(Path.Combine(Path.GetDirectoryName(solutionFilePath), x)));
        }

        private static bool IsProjectOrSolutionFile(string relativeFilePath)
        {
            var extension = Path.GetExtension(relativeFilePath);
            return string.Compare(extension, ".csproj", StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(extension, ".sln", StringComparison.OrdinalIgnoreCase) == 0;
        }

        private static string ChangedFilesHistory(string itemPath, string solutionFilePath)
        {
            var relativeFilePath = PathNetCore.GetRelativePath(Path.GetDirectoryName(solutionFilePath), itemPath);
            //return $"git log --name-only --one-line -- {relativeFilePath}";
            return @$"git log --pretty=format:"" % h"" -- {relativeFilePath} | %{{git diff-tree --no-commit-id --name-only -r $_.Trim()}}";
        }
    }
}
