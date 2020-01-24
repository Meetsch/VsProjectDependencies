using nodewave.App.VsProjectDependencies.Model;
using nodewave.App.VsProjectDependencies.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace nodewave.App.VsProjectDependencies.Plugin.NuGet
{
    [Export(typeof(IParser))]
    public class Parser : IParser
    {
        public bool Verbose { get; set; } = true;

        /// <summary>
        /// Scan a folder and parses NuGet compatible projects
        /// </summary>
        /// <param name="graph">The dependency graph to update with the scanned projects</param>
        /// <param name="rootFolder">The root folder to star scanning from (recursive scan)</param>
        /// <param name="projectFilePathExclusions">A list of terms that would exclude a found project file if its path of filename contains the term.</param>
        public void Parse(Graph graph, string rootFolder, IEnumerable<string> projectFilePathExclusions = null)
        {
            if (Verbose)
                Console.WriteLine("Parsing packages.config files");
            foreach (var pk in Directory.GetFiles(rootFolder, "packages.config", SearchOption.AllDirectories)
                .Where(pc => !pc.Contains(".nuget") && (projectFilePathExclusions == null || !projectFilePathExclusions.Any(ex => pc.Contains(ex))) ))
            {
                if (Verbose)
                    Console.WriteLine(" " + Path.GetFullPath(pk));
                var packageprojects = graph.projectsByPath.Values.Where(p => p.Folder.Path == Path.GetDirectoryName(Path.GetFullPath(pk)));
                foreach (var project in packageprojects)
                {
                    if (project == null)
                    {
                        Console.Error.WriteLine("  ERROR Project not found in same folder than package " + pk);
                    }
                    else
                    {
                        foreach (var pr in XDocument.Load(pk).Descendants("package"))
                        {
                            var package = graph.GetOrCreatePackage(pr.Attribute("id").Value, pr.Attribute("version").Value);
                            project.Packages.Add(package);
                        }
                    }
                }
            }
        }
    }
}
