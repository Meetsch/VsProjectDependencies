using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nodewave.App.VsProjectDependencies
{
    public class Options
    {
        [Usage]
        public static IEnumerable<Example> Examples => new List<Example>() {
                new Example("Create a dependencies graph (.dgml file) by scanning Visual Studio projects in the current folder", new Options { Directory = "." })
            };

        [Option('d', nameof(Directory), Required = true, HelpText = "Directory to be scanned (e.g: --" + nameof(Directory) + " \"C:\\My Project\" ).")]
        public string Directory { get; set; }

        [Option('o', nameof(OutputFilename), Default = "Dependencies.dgml", HelpText = "Output DGML Filename (e.g: --" + nameof(OutputFilename) + " \"Dependencies.dgml\" ). This file will be saved at the root of the scanned directory.")]
        public string OutputFilename { get; set; }

        [Option('e', nameof(FilePathsExclusions), Default = new[] { @"\Utility\", @"\Installer", @"\Demo\", @"\Example\", @"\Sample\", @"\PoC\", @"\POC\", @"\Poc\", @"\PerfTest", @"\PerformanceTest", @"\IntegrationTest", "Test.", ".vcxproj", ".vcproj", ".vdproj", ".ndproj", ".wdproj", ".shfbproj" }, HelpText = "A list of terms that would exclude a found project file if its path of filename contains the term. (e.g: --" + nameof(FilePathsExclusions) + " \"\\Utility\\\" \"\\Installer\" \"\\Demo\\\" \"\\Example\\\" \"\\Sample\\\" \"\\PoC\\\" \"\\POC\\\" \"\\Poc\\\" \"\\PerfTest\" \"\\PerformanceTest\" \"\\IntegrationTest\" \"Test.\" \".vcxproj\" \".vcproj\" \".vdproj\" \".ndproj\" \".wdproj\" \".shfbproj\" ).")]
        public IEnumerable<string> FilePathsExclusions { get; set; }

        [Option('h', nameof(HideDirectories), HelpText = "Don't show the directories in the resulting diagram. (e.g: --" + nameof(HideDirectories) + " ).")]
        public bool HideDirectories { get; set; }

        [Option(nameof(Silent), HelpText = "Don't show progress in the output. (e.g: --" + nameof(Silent) + " ).")]
        public bool Silent { get; set; }
    }
}
