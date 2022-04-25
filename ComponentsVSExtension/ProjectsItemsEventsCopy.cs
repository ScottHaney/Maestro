using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Maestro.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ComponentsVSExtension
{
    public class ProjectItemsEventsCopy : IVsTrackProjectDocumentsEvents2
    {
        internal ProjectItemsEventsCopy()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsTrackProjectDocuments2 tpd = VS.GetRequiredService<SVsTrackProjectDocuments, IVsTrackProjectDocuments2>();
            tpd.AdviseTrackProjectDocumentsEvents(this, out _);
        }

        /// <summary>
        /// Fires after project items was renamed
        /// </summary>
        public event Action<AfterRenameProjectItemEventArgs?>? AfterRenameProjectItems;

        /// <summary>
        /// Fires after project items was removed
        /// </summary>
        public event Action<AfterRemoveProjectItemEventArgs?>? AfterRemoveProjectItems;

        /// <summary>
        /// Fires after project items was added
        /// </summary>
        public event Action<IEnumerable<SolutionItem>>? AfterAddProjectItems;

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HandleAddItems(cProjects, cFiles, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HandleAddItems(cProjects, cDirectories, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HandleRemoveItems(cProjects, cFiles, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HandleRemoveItems(cProjects, cDirectories, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            var projectFileManager = new ProjectFileManager();
            for (int i = 0; i < cFiles; i++)
            {
                var oldName = rgszMkOldNames[i];
                var newName = rgszMkNewNames[i];

                var result = VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameOK;
                if (IsInToTagsFolder(newName) && !IsInToTagsFolder(oldName))
                {
                    result = VSQUERYRENAMEFILERESULTS.VSQUERYRENAMEFILERESULTS_RenameNotOK;

                    var targetProjectFile = GetProjectFilePath(newName);

                    var includePath = PathNetCore.GetRelativePath(Path.GetDirectoryName(targetProjectFile), oldName);
                    var linkPath = PathNetCore.GetRelativePath(Path.GetDirectoryName(targetProjectFile), newName);

                    var contentElement = GetContentElement(includePath, linkPath);

                    projectFileManager.AddLinks(targetProjectFile, contentElement);
                }

                pSummaryResult[i] = result;
                rgResults[i] = result;
            }

            return VSConstants.S_OK;
        }

        private static readonly Regex _tagsFolderPathPartRegex = new Regex(@"[/\\]__Tags", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private bool IsInToTagsFolder(string filePath)
        {
            return _tagsFolderPathPartRegex.IsMatch(filePath);            
        }

        private string GetProjectFilePath(string filePath)
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

        private string GetContentElement(string physicalFilePath, string linkPath)
        {
            return @$"<Content Include=""{physicalFilePath}"" Link=""{linkPath}""/>";
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HandleRenamedItems(cProjects, cFiles, rgpProjects, rgFirstIndices, rgszMkOldNames, rgszMkNewNames);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HandleRenamedItems(cProjects, cDirs, rgpProjects, rgFirstIndices, rgszMkOldNames, rgszMkNewNames);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus) => VSConstants.S_OK;

        private void HandleRenamedItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (AfterRenameProjectItems != null)
            {
                List<ProjectItemRenameDetails> renameParams = new();
                for (int projectIndex = 0; projectIndex < cProjects; projectIndex++)
                {
                    int firstIndex = rgFirstIndices[projectIndex];
                    IVsProject vsProject = rgpProjects[projectIndex];
                    IVsHierarchy vsHierarchy = (IVsHierarchy)vsProject;

                    int nextProjectIndex = cItems;
                    if (rgFirstIndices.Length > projectIndex + 1)
                    {
                        nextProjectIndex = rgFirstIndices[projectIndex + 1];
                    }

                    for (int itemIndex = firstIndex; itemIndex < nextProjectIndex; itemIndex++)
                    {
                        string newName = rgszMkNewNames[itemIndex];
                        string oldName = rgszMkOldNames[itemIndex];
                        vsProject.IsDocumentInProject(newName, out _, new VSDOCUMENTPRIORITY[1], out uint itemid);
                        SolutionItem? projectFile = SolutionItem.FromHierarchy(vsHierarchy, itemid);
                        renameParams.Add(new ProjectItemRenameDetails(projectFile, oldName));
                    }
                }

                AfterRenameProjectItems?.Invoke(new AfterRenameProjectItemEventArgs(renameParams.ToArray()));
            }
        }

        private void HandleRemoveItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (AfterRemoveProjectItems != null)
            {
                List<ProjectItemRemoveDetails> removedItems = new();
                for (int projectIndex = 0; projectIndex < cProjects; projectIndex++)
                {
                    int firstIndex = rgFirstIndices[projectIndex];
                    IVsProject vsProject = rgpProjects[projectIndex];
                    IVsHierarchy vsHierarchy = (IVsHierarchy)vsProject;
                    Project? project = SolutionItem.FromHierarchy(vsHierarchy, VSConstants.VSITEMID_ROOT) as Project;

                    int nextProjectIndex = cItems;
                    if (rgFirstIndices.Length > projectIndex + 1)
                    {
                        nextProjectIndex = rgFirstIndices[projectIndex + 1];
                    }

                    for (int itemIndex = firstIndex; itemIndex < nextProjectIndex; itemIndex++)
                    {
                        string itemName = rgpszMkDocuments[itemIndex];
                        removedItems.Add(new ProjectItemRemoveDetails(project, itemName));
                    }
                }

                AfterRemoveProjectItems?.Invoke(new AfterRemoveProjectItemEventArgs(removedItems.ToArray()));
            }
        }

        private void HandleAddItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (AfterAddProjectItems != null)
            {
                List<SolutionItem> addedItems = new();
                for (int projectIndex = 0; projectIndex < cProjects; projectIndex++)
                {
                    int firstIndex = rgFirstIndices[projectIndex];
                    IVsProject vsProject = rgpProjects[projectIndex];
                    IVsHierarchy vsHierarchy = (IVsHierarchy)vsProject;

                    int nextProjectIndex = cItems;
                    if (rgFirstIndices.Length > projectIndex + 1)
                    {
                        nextProjectIndex = rgFirstIndices[projectIndex + 1];
                    }

                    for (int itemIndex = firstIndex; itemIndex < nextProjectIndex; itemIndex++)
                    {
                        string itemName = rgpszMkDocuments[itemIndex];
                        vsProject.IsDocumentInProject(itemName, out _, new VSDOCUMENTPRIORITY[1], out uint itemid);
                        SolutionItem? projectFile = SolutionItem.FromHierarchy(vsHierarchy, itemid);
                        if (projectFile != null)
                            addedItems.Add(projectFile);
                    }
                }

                AfterAddProjectItems?.Invoke(addedItems);
            }
        }
    }
}
