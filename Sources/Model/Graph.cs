using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nodewave.App.VsProjectDependencies.Model
{
    public class Graph
    {
        /// <summary>
        /// Folders scanned, by absolute full path (the full path must be sanitized using Path.GetFullPath() )
        /// </summary>
        public Dictionary<string, Folder> folders { get; } = new Dictionary<string, Folder>();
        /// <summary>
        /// Projects, by absolute full path (the full path must be sanitized using Path.GetFullPath() )
        /// </summary>
        public Dictionary<string, Project> projectsByPath { get; } = new Dictionary<string, Project>();
        /// <summary>
        /// Projects (*.*proj), by Name
        /// </summary>
        public Dictionary<string, Project> projectsByAssemblyName { get; } = new Dictionary<string, Project>();
        /// <summary>
        /// Packages (nuget), by package's NameVersion (use Package.NameVersion)
        /// </summary>
        public Dictionary<string, Package> packages { get; } = new Dictionary<string, Package>();
        /// <summary>
        /// Libraries (dll), by Name
        /// </summary>
        public Dictionary<string, Library> libraries { get; } = new Dictionary<string, Library>();

        //TODO: GetOrCreateProject: add to both projectsByPath and projectsByAssemblyName

        public Library GetOrCreateLibrary(string name, bool isGAC)
        {
            if (name.Contains(","))
                name = name.Split(',').First();
            var lib = this.libraries.ContainsKey(name) ? this.libraries[name] : null;
            if (lib == null) { lib = new Library { Name = name, IsGAC = isGAC }; this.libraries.Add(lib.Name, lib); }
            return lib;
        }

        public Package GetOrCreatePackage(string name, string version)
        {
            var pkg = this.packages.Values.SingleOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && p.Version == version);
            if (pkg == null) { pkg = new Package { Name = name, Version = version }; this.packages.Add(pkg.NameVersion, pkg); }
            return pkg;
        }
    }
}
