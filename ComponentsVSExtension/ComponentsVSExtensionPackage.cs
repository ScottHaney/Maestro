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

namespace TagsVSExtension
{

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.TagsVSExtensionString)]
    public sealed class ComponentsVSExtensionPackage : ToolkitPackage
    {
        private static VisualStudioWorkspace CurrentWorkspace { get; set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var itemsEvents = new ProjectItemsEventsCopy();
            itemsEvents.AfterAddProjectItems += ItemsEvents_AfterAddProjectItems;

            VS.Events.WindowEvents.FrameIsOnScreenChanged += WindowEvents_FrameIsOnScreenChanged;
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

                            var tagsManager = new Maestro.Core.TagsManager(new FileSystem());
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

                    var fileToOpenPath = Path.Combine(Path.GetDirectoryName(ProjectUtils.GetProjectFilePath(docView.FilePath)), File.ReadAllText(docView.FilePath));
                    await VS.Documents.OpenAsync(fileToOpenPath);
                }
            }
        }

        private bool IsFile(string path)
        {
            return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
    }
}