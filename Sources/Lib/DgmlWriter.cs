using nodewave.App.VsProjectDependencies.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace nodewave.App.VsProjectDependencies.Lib
{
    internal class DgmlWriter
    {
        public bool Verbose { get; set; }

        private static readonly XNamespace dgmlns = "http://schemas.microsoft.com/vs/2009/dgml";

        public void GenerateDGML(Graph graph, string filename, bool showDirectories)
        {
            uint expandDirectoryDeepth = 1;

            if (Verbose)
                Console.WriteLine("Generating " + filename);

            List<Folder> foldersWithProjects = new List<Folder>();
            if (showDirectories)
                foldersWithProjects = graph.folders.Values.Where(f => f.CountProjectsRecursive > 0).ToList();

            var directedGraph = new XElement(dgmlns + "DirectedGraph", new XAttribute("GraphDirection", "LeftToRight"));

            var nodes = new XElement(dgmlns + "Nodes");
            if (showDirectories)
                nodes.Add(foldersWithProjects.Select(f => CreateNode(f.Path, "Folder", @"\" + f.Name, f.Deepth < expandDirectoryDeepth ? "Expanded" : "Collapsed")));

            nodes.Add(graph.projectsByPath.Values.Select(p => CreateNode(p.Name, "Project")));
            nodes.Add(graph.libraries.Values.Select(l => CreateNode(l.Name, l.IsGAC ? "GAC Library" : "Library", l.Name.Split(',')[0])));
            nodes.Add(graph.packages.Values.Select(p => CreateNode(p.NameVersion, "Package")));
            nodes.Add(CreateNode("AllProjects", "Project", label: "All Projects", @group: "Expanded"));
            nodes.Add(CreateNode("AllPackages", "Package", label: "All Packages", @group: "Collapsed"));
            nodes.Add(CreateNode("LocalLibraries", "Library", label: "Local Libraries", @group: "Expanded"));
            nodes.Add(CreateNode("GlobalAssemblyCache", "GAC Library", label: "Global Assembly Cache", @group: "Collapsed"));
            directedGraph.Add(nodes);

            var links = new XElement(dgmlns + "Links");
            if (showDirectories)
            {
                foreach (var f in foldersWithProjects.Where(fwp => fwp.Parent == null))
                    links.Add(CreateLink("AllProjects", f.Path, "Contains"));

                foreach (var f in foldersWithProjects)
                    foreach (var sf in f.Subfolders.Where(sf => foldersWithProjects.Exists(fwp => fwp.Path == sf.Path)))
                        links.Add(CreateLink(f.Path, sf.Path, "Contains"));

                foreach (var p in graph.projectsByPath.Values)
                    links.Add(CreateLink(p.Folder.Path, p.Name, "Contains"));
            }
            links.Add(graph.projectsByPath.Values.SelectMany(p => p.Projects.Select(pr => new { Source = p, Target = pr }))
                        .Select(l => CreateLink(l.Source.Name, l.Target.Name, "Project Dependency")));
            links.Add(graph.projectsByPath.Values.SelectMany(p => p.Libraries.Select(l => new { Source = p, Target = l }))
                        .Select(l => CreateLink(l.Source.Name, l.Target.Name, "Library Dependency")));
            links.Add(graph.projectsByPath.Values.SelectMany(p => p.Packages.Select(pa => new { Source = p, Target = pa }))
                        .Select(l => CreateLink(l.Source.Name, l.Target.NameVersion, "Package Dependency")));
            //TODO: Test Package Source Reference
            links.Add(graph.projectsByPath.Values.SelectMany(p => p.Packages.Where(pa => pa.SourceProject != null).Select(pa => new { Source = p, Target = pa.SourceProject }))
                        .Select(l => CreateLink(l.Source.Name, l.Target.Name, "Package Source Dependency")));
            if (showDirectories)
                links.Add(graph.projectsByPath.Values.Where(p => p.Folder == null).Select(p => CreateLink("AllProjects", p.Name, "Contains")));
            else
                links.Add(graph.projectsByPath.Values.Select(p => CreateLink("AllProjects", p.Name, "Contains")));
            links.Add(graph.packages.Values.Select(p => CreateLink("AllPackages", p.NameVersion, "Contains")));
            links.Add(graph.libraries.Values.Where(l => !l.IsGAC).Select(l => CreateLink("LocalLibraries", l.Name, "Contains")));
            links.Add(graph.libraries.Values.Where(l => l.IsGAC).Select(l => CreateLink("GlobalAssemblyCache", l.Name, "Contains")));
            directedGraph.Add(links);

            // No need to declare Categories, auto generated

            directedGraph.Add(
                new XElement(dgmlns + "Styles",
                        CreateNodeStyle("Folder", "Gray"),
                        CreateNodeStyle("Project", "Blue"),
                        CreateLinkStyle("Project Dependency", "Blue"),
                        CreateNodeStyle("Package", "Purple"),
                        CreateLinkStyle("Package Dependency", "Purple"),
                        CreateLinkStyle("Package Source Dependency", "Purple"),
                        CreateNodeStyle("Library", "Green"),
                        CreateLinkStyle("Library Dependency", "Green"),
                        CreateNodeStyle("GAC Library", "LightGreen")
                        )
                    );

            var doc = new XDocument(directedGraph);
            doc.Save(filename);
        }

        private static XElement CreateNode(string name, string category, string label = null, string @group = null)
        {
            var labelAtt = label != null ? new XAttribute("Label", label) : null;
            var groupAtt = @group != null ? new XAttribute("Group", @group) : null;
            return new XElement(dgmlns + "Node", new XAttribute("Id", name), labelAtt, groupAtt, new XAttribute("Category", category));
        }

        private static XElement CreateLink(string source, string target, string category)
        {
            return new XElement(dgmlns + "Link", new XAttribute("Source", source), new XAttribute("Target", target), new XAttribute("Category", category));
        }

        private static XElement CreateNodeStyle(string category, string color)
        {
            return new XElement(dgmlns + "Style", new XAttribute("TargetType", "Node"), new XAttribute("GroupLabel", category), new XAttribute("ValueLabel", "True"),
                new XElement(dgmlns + "Condition", new XAttribute("Expression", "HasCategory('" + category + "')")),
                new XElement(dgmlns + "Setter", new XAttribute("Property", "Background"), new XAttribute("Value", color)));
        }
        private static XElement CreateLinkStyle(string category, string color)
        {
            return new XElement(dgmlns + "Style", new XAttribute("TargetType", "Link"), new XAttribute("GroupLabel", category), new XAttribute("ValueLabel", "True"),
                new XElement(dgmlns + "Condition", new XAttribute("Expression", "HasCategory('" + category + "')")),
                new XElement(dgmlns + "Setter", new XAttribute("Property", "Stroke"), new XAttribute("Value", color)));
        }
    }
}
