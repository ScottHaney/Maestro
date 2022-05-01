using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Maestro.Core
{
    public interface IItem
    {
        string FilePath { get; }
    }

    public interface IProjectItem : IItem
    {
        IProject Project { get; }
        string GetRelativeFilePath();
    }

    public class ProjectItem : IProjectItem
    {
        public string FilePath { get; private set; }
        public IProject Project { get; private set; }

        public ProjectItem(string filePath, IProject project)
        {
            FilePath = filePath;
            Project = project;
        }

        public string GetRelativeFilePath()
            => Project.GetRelativeItemPath(this);
    }

    public interface IProject
    {
        string FolderPath { get; }
        string GetRelativeItemPath(IItem item);
        ProjectIdentifier GetProjectIdentifier(string solutionFilePath);
    }

    public class Project : IProject
    {
        public string FolderPath { get; private set; }
        public string ProjectFilePath { get; private set; }

        public Project(string projectFilePath)
        {
            FolderPath = Path.GetDirectoryName(projectFilePath);
            ProjectFilePath = projectFilePath;
        }

        public string GetRelativeItemPath(IItem item)
            => GetRelativeItemPath(item.FilePath);

        public string GetRelativeItemPath(string filePath)
        {
            return PathNetCore.GetRelativePath(FolderPath, filePath);
        }

        public ProjectIdentifier GetProjectIdentifier(string solutionFilePath)
        {
            return new ProjectIdentifier(PathNetCore.GetRelativePath(Path.GetDirectoryName(solutionFilePath), ProjectFilePath), Guid.NewGuid());
        }
    }
}
