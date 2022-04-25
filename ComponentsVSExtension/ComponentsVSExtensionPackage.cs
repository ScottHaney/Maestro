global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using Microsoft.VisualStudio;

namespace ComponentsVSExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.ComponentsVSExtensionString)]
    public sealed class ComponentsVSExtensionPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                CommandID menuCommandID = new CommandID(PackageGuids.ComponentsVSExtension, PackageIds.AddTagsCommand);
                OleMenuCommand menuItem = new OleMenuCommand(ExecuteAsync, menuCommandID);

                menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;

                mcs.AddCommand(menuItem);
            }
        }

        private async void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            var menuCommand = (OleMenuCommand)sender;

            var isVisible = false;
            var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                var selections = await solutionExplorer.GetSelectionAsync();

                var hasFile = false;
                var hasFolder = false;
                foreach (var selection in selections)
                {
                    if (selection.Type == SolutionItemType.PhysicalFile)
                        hasFile = true;
                    else if (selection.Type == SolutionItemType.PhysicalFolder)
                        hasFolder = true;

                    if (hasFile && hasFolder)
                        break;
                }

                isVisible = (hasFile && hasFolder);
            }

            menuCommand.Visible = isVisible;
            menuCommand.Enabled = isVisible;
        }

        private async void ExecuteAsync(object sender, EventArgs e)
        {

        }
    }
}