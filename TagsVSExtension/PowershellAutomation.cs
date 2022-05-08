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
            string script = $"git log -- {filePath}";
            ps.AddScript(script);
            ps.Runspace.SessionStateProxy.Path.SetLocation(Path.GetDirectoryName(solutionFilePath));

            var settings = new PSInvocationSettings();
            var results = ps.Invoke();
        }
    }
}
