using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class SelectionManager
    {
        public void ItemsSelected(IEnumerable<SelectedFile> files)
        {

        }

        public void ItemsDeselected(IEnumerable<SelectedFile> files)
        {

        }
    }

    public class SelectedFile
    {
        public readonly string FilePath;

        public SelectedFile(string filePath)
        {
            FilePath = filePath;
        }
    }
}
