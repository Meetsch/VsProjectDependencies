using nodewave.App.VsProjectDependencies.PluginInterfaces;
using nodewave.App.VsProjectDependencies.Model;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace nodewave.App.VsProjectDependencies.Plugin.MsBuild2003
{
    [Export(typeof(IParser))]
    public class Parser : IParser
    {
        public bool Verbose { get; set; } = true;

        /// <summary>
        /// Scan a folder and parses MsBuild2003 compatible projects
        /// </summary>
        /// <param name="graph">The dependency graph to update with the scanned projects</param>
        /// <param name="rootFolder">The root folder to star scanning from (recursive scan)</param>
        /// <param name="projectFilePathExclusions">A list of terms that would exclude a found project file if its path of filename contains the term.</param>
        public void Parse(Graph graph, string rootFolder, IEnumerable<string> projectFilePathExclusions = null)
        {
            if (Verbose)
                Console.WriteLine("Scanning Folders");
            graph.folders.Add(rootFolder, new Folder { Path = rootFolder, Name = new DirectoryInfo(rootFolder).Name, Deepth = 0 });
            foreach (var folder in Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories))
            {
                if (Verbose)
                    Console.Write(".");
                var di = new DirectoryInfo(folder);
                var parent = graph.folders.ContainsKey(di.Parent.FullName) ? graph.folders[di.Parent.FullName] : null;
                var newFolder = new Folder { Path = di.FullName, Name = di.Name, Parent = parent, Deepth = parent != null ? parent.Deepth+1 : 1 };
                graph.folders.Add(newFolder.Path, newFolder);
                if (parent != null)
                    parent.Subfolders.Add(newFolder);
            }
            if (Verbose)
                Console.WriteLine();

            if (Verbose)
                Console.WriteLine("Project files found:");
            foreach (var projectFilePath in Directory.GetFiles(rootFolder, "*.*proj", SearchOption.AllDirectories)
                                .Where(pf => projectFilePathExclusions == null || !projectFilePathExclusions.Any(ex => pf.Contains(ex))))
            {
                if (Verbose)
                    Console.WriteLine(" " + projectFilePath);
                var projectFileInfo = new FileInfo(projectFilePath);
                var folder = graph.folders.ContainsKey(projectFileInfo.DirectoryName) ? graph.folders[projectFileInfo.DirectoryName] : null;
                var name = Path.GetFileNameWithoutExtension(projectFileInfo.Name);
                var project = new Project { Path = projectFileInfo.FullName, Folder = folder, Name = name };
                folder?.Projects.Add(project);

                try
                {
                    var projectDoc = XDocument.Load(project.Path);
                    XNamespace ns = projectDoc.Root.GetDefaultNamespace();
                    var assemblyName = projectDoc.Descendants(ns + "AssemblyName").FirstOrDefault()?.Value;
                    if (!String.IsNullOrEmpty(assemblyName))
                        project.Name = assemblyName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  WARNING couldn't parse \"" + project.Path + "\" to get assembly name, using filename as fallback to identify the project, some packages dependencies link may fail.");
                }

                if (!graph.projectsByPath.ContainsKey(project.Path))
                    graph.projectsByPath.Add(project.Path, project);
                else
                    Console.Error.WriteLine("  ERROR multiple projects with same path \"" + project.Path + "\" were found, dependency graph will have error because project dependencies will link to the first found.");

                if (!graph.projectsByAssemblyName.ContainsKey(project.Name))
                    graph.projectsByAssemblyName.Add(project.Name, project);
                else
                    Console.Error.WriteLine("  ERROR multiple projects with same assembly-name \"" + project.Name + "\" were found, dependency graph will have error because package dependencies will link the first found.");

            }

            // Get all projects, local libraries and GAC references
            if (Verbose)
                Console.WriteLine("Parsing Project files");
            foreach (var project in graph.projectsByPath.Values)
            {
                if (Verbose)
                    Console.WriteLine(" Parsing " + project.Path);
                var projectDoc = XDocument.Load(project.Path);
                XNamespace ns = projectDoc.Root.GetDefaultNamespace();

                //Project References
                foreach (var pr in projectDoc.Descendants(ns + "ProjectReference"))
                {
                    var prjRefPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project.Path), pr.Attribute("Include").Value));
                    var prj = graph.projectsByPath.ContainsKey(prjRefPath) ? graph.projectsByPath[prjRefPath] : null;
                    if (prj != null)
                    {
                        project.Projects.Add(prj);
                    }
                    else
                    {
                        Console.Error.WriteLine("  ERROR " + prjRefPath + " project reference not found");
                    }
                }

                //Library References
                foreach (var r in projectDoc.Descendants(ns + "Reference").Where(r => !r.Value.Contains(@"\packages\")))
                {
                    project.Libraries.Add(graph.GetOrCreateLibrary(r.Attribute("Include").Value, !r.Elements(ns + "HintPath").Any()));
                }

                //Package References
                foreach (var r in projectDoc.Descendants(ns + "PackageReference"))
                {
                    string version = null;
                    if (r.Attribute("Version") != null)
                        version = r.Attribute("Version").Value;
                    else if (r.Elements(ns + "Version").Any())
                        version = r.Elements(ns + "Version").FirstOrDefault()?.Value;

                    project.Packages.Add(graph.GetOrCreatePackage(r.Attribute("Include").Value, version));
                }
            }
        }

    }
}
