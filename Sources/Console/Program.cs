using CommandLine;
using nodewave.App.VsProjectDependencies.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nodewave.App.VsProjectDependencies
{
    internal class Program
    {
        // Based on the following work:
        // http://pascallaurin42.blogspot.com/2014/06/visualizing-nuget-packages-dependencies.html
        // https://gist.github.com/plaurin/b4bc53428f01dc722afb

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>((options) =>
                {
                    VsProjectDependenciesManager manager = new VsProjectDependenciesManager()
                    {
                        Verbose = !options.Silent,
                        ShowDirectories = !options.HideDirectories
                    };

                    manager.Parse(options.Directory, options.OutputFilename, options.FilePathsExclusions);

                    manager.Generate(Path.Combine(options.Directory, options.OutputFilename));
                });

            //Console.ReadKey();
        }

    }
}
