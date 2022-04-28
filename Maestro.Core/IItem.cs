﻿using System;
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
        TagsFolder GetTagsFolder();
        string GetRelativeItemPath(IItem item);
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

        public TagsFolder GetTagsFolder()
        {
            return new TagsFolder(Path.Combine(FolderPath, "__Tags"));
        }

        public string GetRelativeItemPath(IItem item)
        {
            return PathNetCore.GetRelativePath(FolderPath, item.FilePath);
        }
    }
}
