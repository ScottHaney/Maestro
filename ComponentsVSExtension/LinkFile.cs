using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsVSExtension
{
    public class LinkFile
    {
        public readonly string FilePath;
        public string LinkedFilePath => Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));

        private LinkFile(string filePath)
        {
            FilePath = filePath;
        }

        public static bool TryParse(string filePath, out LinkFile linkFile)
        {
            if (string.Compare(Path.GetExtension(filePath), ".link", StringComparison.OrdinalIgnoreCase) == 0)
            {
                linkFile = new LinkFile(filePath);
                return true;
            }
            else
            {
                linkFile = null;
                return false;
            }

        }
    }
}
