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

namespace TagsVSExtension
{

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.TagsVSExtensionString)]
    public sealed class TagsVSExtensionPackage : ToolkitPackage
    {
        public static VisualStudioWorkspace CurrentWorkspace { get; set; }

        private DTE2 _dte;
        private List<object> _eventRefs = new List<object>();

        private SelectionManager _selectionManager;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            _selectionManager = new SelectionManager();

            _dte = await GetServiceAsync(typeof(DTE)) as DTE2;
            _eventRefs.Add(_dte.Events.SelectionEvents);
            _dte.Events.SelectionEvents.OnChange += SelectionEvents_OnChange;

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var itemsEvents = new ProjectItemsEventsCopy();
            itemsEvents.AfterAddProjectItems += ItemsEvents_AfterAddProjectItems;

            VS.Events.WindowEvents.FrameIsOnScreenChanged += WindowEvents_FrameIsOnScreenChanged;

            var componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            CurrentWorkspace = componentModel.GetService<VisualStudioWorkspace>();
        }

        private SelectedItems _previouslySelectedItems = null;

        private async void SelectionEvents_OnChange()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _selectionManager.ItemsDeselected(ToSelectedFiles(_previouslySelectedItems));

            if (_previouslySelectedItems != null && _previouslySelectedItems.Count == 1)
            {
                var previous = _previouslySelectedItems.Cast<SelectedItem>().Single();
                if (previous != null)
                    RemoveLinkFiles(previous);
            }

            var selections = _dte.SelectedItems;
            _previouslySelectedItems = selections;

            _selectionManager.ItemsSelected(ToSelectedFiles(selections));

            if (selections.Count == 1)
            {
                var selectedItem = selections.Cast<SelectedItem>().Single();
                if (selectedItem.ProjectItem != null && CurrentWorkspace != null && !LinkFile.TryParse(selectedItem.ProjectItem.FileNames[0], out var _))
                {
                    var linkFilePath = await CreateLinkFileAsync(selectedItem);

                    if (!ProjectAlreadyHasLink(selectedItem, linkFilePath))
                    {
                        var newItem = selectedItem.ProjectItem.ProjectItems.AddFromFile(linkFilePath);
                    }
                }
            }
        }

        private IEnumerable<SelectedFile> ToSelectedFiles(SelectedItems selectedItems)
        {
            return selectedItems == null
                ? Enumerable.Empty<SelectedFile>()
                : selectedItems.Cast<SelectedItem>().Select(x => ToSelectedFile(x));
        }

        private SelectedFile ToSelectedFile(SelectedItem selectedItem)
        {
            return new SelectedFile(selectedItem.Name);
        }

        private void RemoveLinkFiles(SelectedItem selectedItem)
        {
            if (selectedItem.ProjectItem == null)
                return;

            var collectionItems = selectedItem.ProjectItem.Collection;

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

        private bool ProjectAlreadyHasLink(SelectedItem selectedItem, string linkFilePath)
        {
            var collectionItems = selectedItem.ProjectItem.Collection
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

        private async Task<string> CreateLinkFileAsync(SelectedItem selectedItem)
        {
            var projectItem = await SelectedItemToProjectItemAsync(selectedItem);
            var pathToLinkFile = GetPathToSaveLinkFileTo(projectItem);

            var linkFile = new LinkFile(pathToLinkFile);
            linkFile.Save(new FileSystem(),
                projectItem.GetLinkFileContent());

            return pathToLinkFile;
        }

        private async Task<Maestro.Core.ProjectItem> SelectedItemToProjectItemAsync(SelectedItem selectedItem)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var fileName = selectedItem.ProjectItem.FileNames[0];

            return new Maestro.Core.ProjectItem(fileName,
                selectedItem.ProjectItem.ContainingProject.FileName,
                CurrentWorkspace.CurrentSolution.FilePath);
        }

        private string GetPathToSaveLinkFileTo(Maestro.Core.ProjectItem projectItem)
        {
            var solutionFilePath = CurrentWorkspace.CurrentSolution.FilePath;
            var directory = Path.Combine(Path.GetDirectoryName(solutionFilePath), "__Links");

            return Path.Combine(directory, Path.GetFileName(projectItem.FileName) + ".link");
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

                    await VS.Documents.OpenAsync(contents.GetFullFilePath(CurrentWorkspace.CurrentSolution.FilePath));
                }
            }
        }

        private bool IsFile(string path)
        {
            return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
    }
}