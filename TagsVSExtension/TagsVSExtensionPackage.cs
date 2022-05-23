global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Abstractions;
using Maestro.Core;
using Maestro.GitManagement;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Maestro.Core.Links;
using Microsoft.VisualStudio.Threading;
using ProjectItem = EnvDTE.ProjectItem;
using Project = EnvDTE.Project;

namespace TagsVSExtension
{

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.TagsVSExtensionString)]
    public sealed class TagsVSExtensionPackage : ToolkitPackage
    {
        public static VisualStudioWorkspace CurrentWorkspace { get; set; }

        private DTE2 _dte;
        private List<object> _eventRefs = new List<object>();

        private SelectionManager _selectionManager;
        private SelectedItems _previouslySelectedItems = null;

        private WhenAreLinkFilesShown _whenAreLinkFilesShown;
        private WhichItemsShouldBeLinked _whichItemsShouldBeLinked;
        private HowToShowLinkFiles _howToShowLinkFiles;
        private HowAreLinkedFilesStored _howAreLinkedFilesStored;

        private IVisualWorkspace _visualWorkspace;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            CurrentWorkspace = componentModel.GetService<VisualStudioWorkspace>();

            var isUnderGitSourceControl = Directory.Exists(Path.Combine(Path.GetDirectoryName(CurrentWorkspace.CurrentSolution.FilePath), ".git"));
            if (isUnderGitSourceControl)
            {
                _dte = await GetServiceAsync(typeof(DTE)) as DTE2;

                //_selectionManager = new SelectionManager(new VisualStudioSolution(CurrentWorkspace, _dte));
                //_eventRefs.Add(_dte.Events.SelectionEvents);
                //_dte.Events.SelectionEvents.OnChange += SelectionEvents_OnChange;

                _visualWorkspace = new VisualStudioVisualWorkspace(_dte, CurrentWorkspace, JoinableTaskFactory);

                _howToShowLinkFiles = new HowToShowLinkFiles(_visualWorkspace,
                    CurrentWorkspace,
                    new FileSystem());
                _howAreLinkedFilesStored = new HowAreLinkedFilesStored(new FileSystem(),
                    CurrentWorkspace);

                _whenAreLinkFilesShown = new WhenAreLinkFilesShown(_visualWorkspace, _howAreLinkedFilesStored);
                _whichItemsShouldBeLinked = new WhichItemsShouldBeLinked(CurrentWorkspace);

                _whenAreLinkFilesShown.ShowLinks += (sender, args) =>
                {
                    foreach (var item in args)
                    {
                        if (item.IsLinkFile() || item.IsProjectOrSolutionFile())
                            continue;

                        var topLinks = PowershellAutomation.GetHistoryFromGit(CurrentWorkspace.CurrentSolution.FilePath,
                            item.GetFullItemPath(CurrentWorkspace.CurrentSolution.FilePath))
                        .Select(x => new Maestro.Core.ProjectItem(x))
                        .Take(5)
                        .ToList();

                        //Only choose distinct file names since they have to be saved in the same folder
                        topLinks.GroupBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                            .Select(x => x.First())
                            .ToList();

                        var suggestedLinks = _whichItemsShouldBeLinked.GetLinks(item);
                        var linksToShow = topLinks.Concat(suggestedLinks).Distinct();
                        var storedLinks = _howAreLinkedFilesStored.StoreLinkFiles(item, linksToShow).ToList();

                        _howToShowLinkFiles.ShowLinks(item, storedLinks);
                    }
                };

                _whenAreLinkFilesShown.HideLinks += (sender, args) =>
                {
                    _howAreLinkedFilesStored.DeleteLinksForItems(args);
                    _howToShowLinkFiles.HideLinks(args);
                };

                var itemsEvents = new ProjectItemsEventsCopy();
                itemsEvents.AfterAddProjectItems += ItemsEvents_AfterAddProjectItems;

                VS.Events.WindowEvents.FrameIsOnScreenChanged += WindowEvents_FrameIsOnScreenChanged;
            }
        }

        private Maestro.Core.ProjectItem ToProjectItem(EnvDTE.ProjectItem projectItem)
        {
            var fileName = projectItem.FileNames[1];

            if (!string.IsNullOrEmpty(fileName))
            {
                return new Maestro.Core.ProjectItem(fileName,
                        projectItem.ContainingProject.FileName,
                        CurrentWorkspace.CurrentSolution.FilePath);
            }
            else
                return null;
        }

        private async void SelectionEvents_OnChange()
        {
            if (_selectionManager == null)
                return;

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _selectionManager.ItemsDeselected(SelectedItemsToProjectItems(_previouslySelectedItems).ToList());

            var selections = _dte.SelectedItems;
            _previouslySelectedItems = selections;

            await _selectionManager.ItemsSelectedAsync(SelectedItemsToProjectItems(selections).ToList());
        }

        private IEnumerable<Maestro.Core.ProjectItem> SelectedItemsToProjectItems(SelectedItems selectedItems)
        {
            if (selectedItems == null)
                return Enumerable.Empty<Maestro.Core.ProjectItem>();

            var results = new List<Maestro.Core.ProjectItem>();
            foreach (SelectedItem selectedItem in selectedItems)
            {
                if (selectedItem.ProjectItem == null)
                    continue;

                var fileName = selectedItem.ProjectItem.FileNames[0];

                var item = new Maestro.Core.ProjectItem(fileName,
                    selectedItem.ProjectItem.ContainingProject.FileName,
                    CurrentWorkspace.CurrentSolution.FilePath);

                results.Add(item);
            }

            return results;
        }

        private async void ItemsEvents_AfterAddProjectItems(IEnumerable<SolutionItem> obj)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            //Logic to create a link when files are copied between projects. This should really be moved into the 
            //query event rather than the afteradd event but leave it as is for now

            var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                var selections = (await solutionExplorer.GetSelectionAsync()).ToList();
                var newItems = obj.ToList();

                if (selections.Any() && selections.All(x => IsFile(x.FullPath)) && newItems.All(x => IsFile(x.FullPath)))
                {
                    var selectionsNames = new HashSet<string>(selections.Select(x => Path.GetFileName(x.FullPath)), StringComparer.OrdinalIgnoreCase);
                    var newItemsNames = new HashSet<string>(newItems.Select(x => Path.GetFileName(x.FullPath)), StringComparer.OrdinalIgnoreCase);

                    if (selectionsNames.Count == selections.Count && newItemsNames.Count == newItems.Count && selectionsNames.SequenceEqual(newItemsNames))
                    {
                        var selectionsNamesMap = selections.ToDictionary(x => Path.GetFileName(x.FullPath), x => x.FullPath);
                        var newItemsNamesMap = newItems.ToDictionary(x => Path.GetFileName(x.FullPath), x => x.FullPath);

                        foreach (var item in newItemsNames)
                        {
                            var oldPath = selectionsNamesMap[item];
                            var newPath = newItemsNamesMap[item];

                            var tagsManager = new Maestro.Core.TagsManager(new FileSystem(), CurrentWorkspace.CurrentSolution.FilePath);
                            if (tagsManager.IsInTagsFolder(newPath) && !tagsManager.IsInTagsFolder(oldPath))
                            {
                                var targetProjectFile = ProjectUtils.GetProjectFilePath(newPath);
                                var sourceProjectFile = ProjectUtils.GetProjectFilePath(oldPath);

                                if (string.Compare(targetProjectFile, sourceProjectFile, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    File.Delete(newPath);
                                    tagsManager.CreateLink(targetProjectFile, oldPath, newPath);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void WindowEvents_FrameIsOnScreenChanged(FrameOnScreenEventArgs args)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            if (LinkFile.TryParse(args.Frame.Caption, out var _))
            {
                var docView = await args.Frame.GetDocumentViewAsync();
                if (LinkFile.TryParse(docView.FilePath, out var linkFile))
                {
                    await args.Frame.HideAsync();

                    var contents = JsonConvert.DeserializeObject<LinkFileContent>(File.ReadAllText(docView.FilePath));

                    var filePath = contents.GetFullFilePath(CurrentWorkspace.CurrentSolution.FilePath);
                    if (File.Exists(filePath))
                        await VS.Documents.OpenAsync(filePath);
                }
            }
        }

        private bool IsFile(string path)
        {
            if (!File.Exists(path))
                return false;

            return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
    }

    public class VisualStudioVisualWorkspace : IVisualWorkspace
    {
        public event EventHandler<List<Maestro.Core.ProjectItem>> ItemsSelected;
        public event EventHandler<List<Maestro.Core.ProjectItem>> ItemsUnselected;

        private readonly DTE2 _dte;
        private readonly Workspace _workspace;
        private readonly JoinableTaskFactory _joinableTaskFactory;

        private SelectedItems _previouslySelectedItems;
        private readonly List<object> _eventRefs = new List<object>();

        public VisualStudioVisualWorkspace(DTE2 dte,
            Workspace workspace,
            JoinableTaskFactory joinableTaskFactory)
        {
            _dte = dte;
            _workspace = workspace;
            _joinableTaskFactory = joinableTaskFactory;

            _eventRefs.Add(_dte.Events.SelectionEvents);
            _dte.Events.SelectionEvents.OnChange += SelectionEvents_OnChange;
        }

        private async void SelectionEvents_OnChange()
        {
            await _joinableTaskFactory.SwitchToMainThreadAsync();

            //ItemsUnselected?.Invoke(this, SelectedItemsToProjectItems(_previouslySelectedItems).ToList());

            var currentSelections = _dte.SelectedItems;
            ItemsSelected?.Invoke(this, SelectedItemsToProjectItems(currentSelections).ToList());

            _previouslySelectedItems = currentSelections;
        }

        public void ShowLinks(Maestro.Core.ProjectItem projectItem, IEnumerable<Maestro.Core.Links.StoredLinkFile> linkedItems)
        {
            if (!projectItem.IsLinkFile())
            {
                foreach (var linkedItem in linkedItems)
                {
                    if (!ProjectAlreadyHasLink(projectItem, linkedItem))
                        AddProjectItem(projectItem, linkedItem);
                }
            }
        }

        public void HideLinks(IEnumerable<Maestro.Core.ProjectItem> projectItems)
        {
            foreach (var projectItem in projectItems)
            {
                var selectedItem = ToProjectItem(projectItem);
                if (selectedItem == null)
                    return;

                var collectionItems = selectedItem.ProjectItems;

                //This is a 1 based indexing system...
                for (int i = collectionItems.Count; i > 0; i--)
                {
                    var collectionItem = collectionItems.Item(i);
                    if (collectionItem != null)
                    {
                        var dynamicItem = (dynamic)collectionItem;
                        var name = dynamicItem?.Name;

                        if (string.Compare(".link", Path.GetExtension(name), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            collectionItem.Remove();
                        }
                    }
                }
            }
        }

        private bool ProjectAlreadyHasLink(Maestro.Core.ProjectItem coreProjectItem, Maestro.Core.Links.StoredLinkFile link)
        {
            var visualProjectItem = ToProjectItem(coreProjectItem);
            var collectionItems = visualProjectItem.Collection
                .OfType<object>();

            var linkFileName = Path.GetFileName(link.ProjectItem.GetFullItemPath(_workspace.CurrentSolution.FilePath) + ".link");
            foreach (var item in collectionItems)
            {
                var dynamicItem = (dynamic)item;
                var name = dynamicItem?.Name;

                if (string.Compare(linkFileName, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }

            return false;
        }
        private void AddProjectItem(Maestro.Core.ProjectItem projectItem, Maestro.Core.Links.StoredLinkFile linkedItem)
        {
            var vsProjectItem = ToProjectItem(projectItem);
            if (vsProjectItem == null)
                return;

            var vsLinkedItem = vsProjectItem.ProjectItems.AddFromFile(linkedItem.LinkFilePath);

            bool mayNeedAttributeSet = vsLinkedItem.ContainingProject.IsKind(
                    ProjectTypes.DOTNET_Core,
                    ProjectTypes.UNIVERSAL_APP,
                    ProjectTypes.SHARED_PROJECT,
                    ProjectTypes.NETSTANDARD);

            //if (!(mayNeedAttributeSet && SetDependentUpon(vsLinkedItem, vsProjectItem.Name)))
            //{
                //vsLinkedItem.Remove();
                //vsProjectItem.ProjectItems.AddFromFile(linkedItem.LinkFilePath);
            //}
        }

        private static bool SetDependentUpon(ProjectItem item, string value)
        {
            if (item.ContainsProperty("DependentUpon"))
            {
                item.Properties.Item("DependentUpon").Value = value;
                return true;
            }

            return false;
        }

        private EnvDTE.ProjectItem ToProjectItem(Maestro.Core.ProjectItem projectItem)
        {
            if (projectItem == null)
                return null;

            return _dte.Solution.FindProjectItem(projectItem.GetFullItemPath(_workspace.CurrentSolution.FilePath));
        }

        private IEnumerable<Maestro.Core.ProjectItem> SelectedItemsToProjectItems(SelectedItems selectedItems)
        {
            if (selectedItems == null)
                return Enumerable.Empty<Maestro.Core.ProjectItem>();

            var results = new List<Maestro.Core.ProjectItem>();
            foreach (SelectedItem selectedItem in selectedItems)
            {
                if (selectedItem.ProjectItem == null)
                    continue;

                var fileName = selectedItem.ProjectItem.FileNames[0];

                var item = new Maestro.Core.ProjectItem(fileName,
                    selectedItem.ProjectItem.ContainingProject.FileName,
                    _workspace.CurrentSolution.FilePath);

                results.Add(item);
            }

            return results;
        }

        public void ClearLinks(IEnumerable<ExistingLinkFile> existingLinks)
        {
            foreach (var existingLink in existingLinks)
            {
                var projectItem = _dte.Solution.FindProjectItem(existingLink.FilePath);
                if (projectItem != null)
                    projectItem.Remove();
            }
        }
    }

    static class ManualNester
    {
        private const string CordovaKind = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
        public static void Nest(ProjectItem parent, IEnumerable<ProjectItem> items)
        {
            if (parent == null)
                return;

            foreach (ProjectItem item in items)
            {
                string path = item.FileNames[0];

                bool mayNeedAttributeSet = item.ContainingProject.IsKind(
                    ProjectTypes.DOTNET_Core,
                    ProjectTypes.UNIVERSAL_APP,
                    ProjectTypes.SHARED_PROJECT,
                    ProjectTypes.NETSTANDARD);

                if (!(mayNeedAttributeSet && SetDependentUpon(item, parent.Name)))
                {
                    item.Remove();
                    parent.ProjectItems.AddFromFile(path);
                }
            }
        }

        public static void UnNest(ProjectItem item)
        {
            foreach (ProjectItem child in item.ProjectItems)
            {
                UnNest(child);
            }

            UnNestItem(item);
        }

        private static void UnNestItem(ProjectItem item)
        {
            string path = item.FileNames[0];
            object parent = item.Collection.Parent;

            bool shouldAddToParentItem = item.ContainingProject.Kind == CordovaKind;

            while (parent != null)
            {
                var pi = parent as ProjectItem;

                if (pi != null)
                {
                    if (!Path.HasExtension(pi.FileNames[0]))
                    {
                        object itemType = item.Properties.Item("ItemType").Value;

                        DeleteAndAdd(item, path);

                        ProjectItem newItem;
                        if (shouldAddToParentItem)
                        {
                            newItem = pi.ProjectItems.AddFromFile(path);
                        }
                        else
                        {
                            newItem = pi.ContainingProject.ProjectItems.AddFromFile(path);
                        }
                        newItem.Properties.Item("ItemType").Value = itemType;
                        break;
                    }

                    parent = pi.Collection.Parent;
                }
                else
                {
                    var pj = parent as Project;
                    if (pj != null)
                    {
                        object itemType = item.Properties.Item("ItemType").Value;

                        DeleteAndAdd(item, path);

                        ProjectItem newItem = pj.ProjectItems.AddFromFile(path);
                        newItem.Properties.Item("ItemType").Value = itemType;
                        break;
                    }
                }
            }
        }

        private static void DeleteAndAdd(ProjectItem item, string path)
        {
            if (!File.Exists(path))
                return;

            string temp = Path.GetTempFileName();
            File.Copy(path, temp, true);
            item.Delete();
            File.Copy(temp, path);
            File.Delete(temp);
        }

        private static void RemoveDependentUpon(ProjectItem item)
        {
            SetDependentUpon(item, null);
        }

        private static bool SetDependentUpon(ProjectItem item, string value)
        {
            if (item.ContainsProperty("DependentUpon"))
            {
                item.Properties.Item("DependentUpon").Value = value;
                return true;
            }

            return false;
        }
    }

    static class Helpers
    {
        public static bool ContainsProperty(this ProjectItem projectItem, string propertyName)
        {
            if (projectItem.Properties != null)
            {
                foreach (Property item in projectItem.Properties)
                {
                    if (item != null && item.Name == propertyName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsKind(this Project project, params string[] kindGuids)
        {
            foreach (var guid in kindGuids)
            {
                if (project.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }

    public static class ProjectTypes
    {
        public const string ASPNET_5 = "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}";
        public const string DOTNET_Core = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
        public const string WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        public const string UNIVERSAL_APP = "{262852C6-CD72-467D-83FE-5EEB1973A190}";
        public const string NODE_JS = "{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}";
        public const string SSDT = "{00d1a9c2-b5f0-4af3-8072-f6c62b433612}";
        public const string SHARED_PROJECT = "{D954291E-2A0B-460D-934E-DC6B0785DB48}";
        public const string NETSTANDARD = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
    }

    public class VisualStudioSolution : IVisualStudioSolution
    {
        private readonly Workspace _workspace;
        private readonly DTE2 _dte;

        public VisualStudioSolution(Workspace workspace,
            DTE2 dte)
        {
            _workspace = workspace;
            _dte = dte;
        }

        public void AddProjectItem(Maestro.Core.ProjectItem projectItem, string linkFilePath)
        {
            ToProjectItem(projectItem).ProjectItems.AddFromFile(linkFilePath);
        }

        public async Task<string> CreateLinkFileAsync(Maestro.Core.ProjectItem coreProjectItem)
        {
            var projectItem = SelectedItemsToProjectItems(coreProjectItem).Single();
            var pathToLinkFile = GetPathToSaveLinkFileTo(projectItem);

            var linkFile = new LinkFile(pathToLinkFile);
            linkFile.Save(new FileSystem(),
                projectItem.GetLinkFileContent());

            return pathToLinkFile;
        }

        public bool ProjectAlreadyHasLink(Maestro.Core.ProjectItem coreProjectItem, string linkFilePath)
        {
            var projectItem = ToProjectItem(coreProjectItem);
            var collectionItems = projectItem.Collection
                .OfType<object>();

            var linkFileName = Path.GetFileName(linkFilePath);
            foreach (var item in collectionItems)
            {
                var dynamicItem = (dynamic)item;
                var name = dynamicItem?.Name;

                if (string.Compare(linkFileName, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }

            return false;
        }

        public void RemoveLinkFiles(Maestro.Core.ProjectItem coreProjectItem)
        {
            var selectedItem = ToProjectItem(coreProjectItem);
            if (selectedItem == null)
                return;

            var collectionItems = selectedItem.Collection;

            for (int i = collectionItems.Count - 1; i >= 0; i++)
            {
                var collectionItem = collectionItems.Item(i);
                if (collectionItem != null)
                {
                    var dynamicItem = (dynamic)collectionItem;
                    var name = dynamicItem?.Name;

                    if (string.Compare(".link", Path.GetExtension(name), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        collectionItem.Remove();
                    }
                }
            }
        }

        private IEnumerable<Maestro.Core.ProjectItem> SelectedItemsToProjectItems(params Maestro.Core.ProjectItem[] selectedItems)
        {
            var results = new List<Maestro.Core.ProjectItem>();
            foreach (var selectedItem in selectedItems)
            {
                var projectItem = ToProjectItem(selectedItem);
                var fileName = projectItem.FileNames[0];

                var item = new Maestro.Core.ProjectItem(fileName,
                    projectItem.ContainingProject.FileName,
                    _workspace.CurrentSolution.FilePath);

                results.Add(item);
            }

            return results;
        }

        private string GetPathToSaveLinkFileTo(Maestro.Core.ProjectItem projectItem)
        {
            var solutionFilePath = _workspace.CurrentSolution.FilePath;
            var directory = Path.Combine(Path.GetDirectoryName(solutionFilePath), "__Links");

            return Path.Combine(directory, Path.GetFileName(projectItem.FileName) + ".link");
        }

        private EnvDTE.ProjectItem ToProjectItem(Maestro.Core.ProjectItem projectItem)
        {
            if (projectItem == null)
                return null;

            return _dte.Solution.FindProjectItem(projectItem.GetFullItemPath(_workspace.CurrentSolution.FilePath));
        }
    }
}