using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;

namespace Maestro.GitManagement
{
    public interface IGitHistoryManager
    {
        IEnumerable<GitCommit> GetHistoryForFile(string filePath);
    }

    public class GitHistoryManager : IGitHistoryManager
    {
        private readonly string _gitRepoBasePath;

        public GitHistoryManager(string solutionFilePath)
        {
            _gitRepoBasePath = Path.GetDirectoryName(solutionFilePath);
        }

        public IEnumerable<GitCommit> GetHistoryForFile(string filePath)
        {
            /*using (PowerShell powershell = PowerShell.Create())
            {
                // this changes from the user folder that PowerShell starts up with to your git repository
                powershell.AddScript($"cd {_gitRepoBasePath}");

                powershell.AddScript($@"git log {filePath}");

                var results = powershell.Invoke().ToList();
            }*/

            return Enumerable.Empty<GitCommit>();
        }
    }

    public class GitCommit
    {
        public readonly List<GitHistoryFile> Files;

        public GitCommit(List<GitHistoryFile> files)
        {
            Files = files;
        }
    }

    public class GitHistoryFile
    {
        public readonly string FilePath;

        public GitHistoryFile(string filePath)
        {
            FilePath = filePath;
        }
    }
}
