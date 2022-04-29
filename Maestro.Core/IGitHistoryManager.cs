using LibGit2Sharp;
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
            using (var repo = new Repository(_gitRepoPath))
            {
                foreach (var commit in repo.Commits.QueryBy(PathNetCore.GetRelativePath(_gitRepoPath, filePath)))
                    yield return new GitCommit(commit.Commit.Tree.Select(x => new GitHistoryFile(x.Path)).ToList());
            }
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
