using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Maestro.Core
{
    public interface ITagManager
    {
        IEnumerable<ComponentTag> GetTags();
    }

    public class TagManager : ITagManager
    {
        private readonly string _baseFolder;

        public TagManager(string baseFolder)
        {
            _baseFolder = baseFolder;
        }

        public IEnumerable<ComponentTag> GetTags()
        {
            foreach (var file in Directory.EnumerateFiles(_baseFolder))
            {
                var contents = File.ReadAllLines(file);

                var tagName = contents[0];

                var keys = new List<ComponentKey>();
                foreach (var item in contents[1].Split('|'))
                {
                    var pieces = item.Split('/');
                    keys.Add(new ComponentKey(pieces[0], pieces[1]));
                }

                yield return new ComponentTag(Guid.Parse(Path.GetFileNameWithoutExtension(file)), tagName, keys);
            }
        }
    }

    public class ComponentKey
    {
        public readonly string ProjectName;
        public readonly string FileName;

        public ComponentKey(string projectName, string fileName)
        {
            ProjectName = projectName;
            FileName = fileName;
        }
    }

    public class ComponentTag
    {
        public readonly Guid Id;
        public readonly string Name;

        public readonly List<ComponentKey> Components;

        public ComponentTag(Guid id, string name, List<ComponentKey> components)
        {
            Id = id;
            Name = name;
            Components = components;
        }
    }
}
