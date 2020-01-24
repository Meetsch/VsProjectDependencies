using System.Collections.Generic;

namespace nodewave.App.VsProjectDependencies.Model
{
    public class Project
    {
        public Project()
        {
            Projects = new List<Project>();
            Libraries = new List<Library>();
            Packages = new List<Package>();
        }
        public string Path { get; set; }
        public Folder Folder { get; set; }
        public string Name { get; set; }
        public List<Project> Projects { get; private set; }
        public List<Library> Libraries { get; private set; }
        public List<Package> Packages { get; private set; }
    }
}
