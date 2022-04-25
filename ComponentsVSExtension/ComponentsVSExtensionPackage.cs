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

namespace ComponentsVSExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.ComponentsVSExtensionString)]
    public sealed class ComponentsVSExtensionPackage : ToolkitPackage
    {
        private static VisualStudioWorkspace CurrentWorkspace { get; set; }
        
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);



            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                CommandID menuCommandID = new CommandID(PackageGuids.ComponentsVSExtension, PackageIds.AddTagsCommand);
                OleMenuCommand menuItem = new OleMenuCommand(ExecuteAsync, menuCommandID);

                menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
                mcs.AddCommand(menuItem);

                //VS.Events.ProjectItemsEvents.AfterRenameProjectItems
                //    += ProjectItemsEvents_AfterRenameProjectItems;

                var itemsEvents = new ProjectItemsEventsCopy();
                /*var dte = await GetServiceAsync(typeof(DTE)) as DTE2;
                dte.Events.CommandEvents.BeforeExecute += CommandEvents_BeforeExecute;*/

                /*var cmd = mcs.FindCommand(null) as OleMenuCommand;
                cmd.BeforeQueryStatus ...*/
            }
        }

        private void ProjectItemsEvents_AfterRenameProjectItems(AfterRenameProjectItemEventArgs obj)
        {

        }

        private void CommandEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            System.Diagnostics.Debug.WriteLine($"Command executed: {Guid}, {ID}");
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
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (CurrentWorkspace == null)
            {
                var componentModel = (IComponentModel)(await this.GetServiceAsync(typeof(SComponentModel)));
                CurrentWorkspace = componentModel.GetService<VisualStudioWorkspace>();
            }

            var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                var selections = await solutionExplorer.GetSelectionAsync();

                var folders = selections
                    .OfType<PhysicalFolder>();

                var files = selections
                    .OfType<PhysicalFile>()
                    .ToList();

                foreach (var folder in folders.Cast<PhysicalFolder>())
                {
                    var projectNode = folder.ContainingProject;

                    foreach (var file in files)
                    {
                        System.IO.File.WriteAllText(projectNode.FullPath, System.IO.File.ReadAllText(projectNode.FullPath) + "<!---->");
                    }
                }
            }
        }

        private string CreateContentLink(string linkPath, string physicalFilePath)
        {
            return @$"<ContentItem Include=""{physicalFilePath}"" Link=""{linkPath}""/>";
        }

        //private void CreateFileLinks(string folderPath, )
    }
}