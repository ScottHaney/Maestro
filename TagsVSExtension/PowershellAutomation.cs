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
        public static void GetHistoryFromGit(string solutionFilePath, string filePath)
        {
            var ps = PowerShell.Create();
            string script = ChangedFilesHistory(filePath, solutionFilePath);
            ps.AddScript(script);
            ps.Runspace.SessionStateProxy.Path.SetLocation(Path.GetDirectoryName(solutionFilePath));

            var settings = new PSInvocationSettings();
            var results = ps.Invoke();

            var top5 = results.Select(x => x.ToString())
                .GroupBy(x => x)
                .OrderBy(x => x.Count())
                .Take(5)
                .Select(x => x.Key)
                .ToList();
        }

        private static string ChangedFilesHistory(string itemPath, string solutionFilePath)
        {
            var relativeFilePath = PathNetCore.GetRelativePath(Path.GetDirectoryName(solutionFilePath), itemPath);
            //return $"git log --name-only --one-line -- {relativeFilePath}";
            return @$"git log --pretty=format:"" % h"" -- {relativeFilePath} | %{{git diff-tree --no-commit-id --name-only -r $_.Trim()}}";
        }
    }
}
