using nodewave.App.VsProjectDependencies.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace nodewave.App.VsProjectDependencies.PluginInterfaces
{
    public interface IParser
    {
        bool Verbose { get; set; }
        void Parse(Graph graph, string rootFolder, IEnumerable<string> packageExtensionExclusions = null);
    }
}
