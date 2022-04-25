using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Maestro.Core
{
    public class ProjectFileManager
    {
        public void AddLinks(string filePath, string link)
        {
            var xDoc = new XmlDocument();
            xDoc.Load(filePath);

            var itemGroup = xDoc.CreateElement("ItemGroup");
            itemGroup.InnerXml = link;
            xDoc.DocumentElement.AppendChild(itemGroup);

            xDoc.Save(filePath);
        }
    }
}
