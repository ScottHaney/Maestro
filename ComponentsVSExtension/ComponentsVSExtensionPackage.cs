global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading;

namespace ComponentsVSExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(MyToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.ComponentsVSExtensionString)]
    public sealed class ComponentsVSExtensionPackage : ToolkitPackage
    {
        public static VisualStudioWorkspace CurrentWorkspace { get; set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();

            var componentModel =  (IComponentModel)this.GetService(typeof(SComponentModel));
            CurrentWorkspace = componentModel.GetService<VisualStudioWorkspace>();

            this.RegisterToolWindows();
        }
    }
}