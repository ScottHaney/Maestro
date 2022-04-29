using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Maestro.Core
{
    public interface IGitHistoryManager
    {
        IEnumerable<GitCommit> GetHistoryForFile(string filePath);
    }

    public class GitHistoryManager : IGitHistoryManager
    {
        private readonly string _gitRepoPath;

        public GitHistoryManager(string gitRepoPath)
        {
            _gitRepoPath = gitRepoPath;
        }

        public IEnumerable<GitCommit> GetHistoryForFile(string filePath)
        {
            throw new NotImplementedException();
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
