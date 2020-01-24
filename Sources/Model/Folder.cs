using System;
using System.Collections.Generic;
using System.Text;

namespace nodewave.App.VsProjectDependencies.Model
{
    public class Folder
    {
        public Folder()
        {
            Subfolders = new List<Folder>();
            Projects = new List<Project>();
        }
        public string Path { get; set; }
        public string Name { get; set; }
        public Folder Parent { get; set; }
        public uint Deepth { get; set; }
        public List<Folder> Subfolders { get; private set; }
        public List<Project> Projects { get; private set; }
        public int CountProjectsRecursive
        {
            get
            {
                //TODO: optimize by caching count results and invalidating cache when a subfolder or a project is added
                int count = Projects.Count;
                foreach (var sf in Subfolders)
                    count += sf.CountProjectsRecursive;
                return count;
            }
        }
    }
}
