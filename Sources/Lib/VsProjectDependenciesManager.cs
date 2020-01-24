using nodewave.App.VsProjectDependencies.Model;
using nodewave.App.VsProjectDependencies.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace nodewave.App.VsProjectDependencies.Lib
{
    public class VsProjectDependenciesManager
    {
        public bool Verbose { get; set; }
        public bool ShowDirectories { get; set; }

        private Graph graph = new Graph();

        /// <summary>
        /// Parses a folder to create a dependency diagram out of the visual studio project file it finds.
        /// </summary>
        /// <param name="rootFolder">The root folder to star scanning from (recursive scan)</param>
        /// <param name="outputFilename">The file path of the diagram file to output to.</param>
        /// <param name="projectFilePathExclusions">A list of terms that would exclude a found project file if its path of filename contains the term.</param>
        public void Parse(string rootFolder, string outputFilename, IEnumerable<string> projectFilePathExclusions = null)
        {
            //Load plugins
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var assemblies = Directory.GetFiles(directory, "nodewave.App.VsProjectDependencies.Plugin.*.dll")
                            .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).Where(ass => ass.ExportedTypes.Count() > 0);
            if (Verbose)
            {
                Console.WriteLine("Possible Plugins found:");
                foreach (var assembly in assemblies)
                {
                    Console.WriteLine(" " + assembly.FullName);
                    foreach (var exportedType in assembly.ExportedTypes)
                    {
                        Console.WriteLine("  " + exportedType.FullName);
                    }
                }
            }
            var configuration = new ContainerConfiguration().WithAssemblies(assemblies);
            var container = configuration.CreateContainer();

            if (Verbose)
                Console.WriteLine("Parsing");
            foreach (var projectParser in container.GetExports<IParser>())
            {
                projectParser.Parse(graph, rootFolder, projectFilePathExclusions);
            }
            if (Verbose)
                Console.WriteLine("Done.");
        }

        private void FinalizeGraph()
        {
            //Package Source Reference (when we have the project source-code of a nuget package, add a link not only to the nuget package but also to its project)
            foreach (var package in graph.packages.Values.ToArray())
            {
                if (graph.projectsByAssemblyName.ContainsKey(package.Name))
                {
                    package.SourceProject = graph.projectsByAssemblyName[package.Name];
                }
            }
        }

        public void Generate(string output)
        {
            FinalizeGraph();

            DgmlWriter writer = new DgmlWriter() { Verbose = this.Verbose };

            writer.GenerateDGML(graph, output, ShowDirectories);
        }

    }
}
