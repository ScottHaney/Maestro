using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class WorkspaceWatcher
    {
        private readonly Workspace _workspace;

        public event EventHandler ComponentRenamed;

        public WorkspaceWatcher(Workspace workspace)
        {
            _workspace = workspace;

            _workspace.WorkspaceChanged += _workspace_WorkspaceChanged;
        }

        private void _workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {

        }
    }
}
