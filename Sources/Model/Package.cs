namespace nodewave.App.VsProjectDependencies.Model
{
    public class Package
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public Project SourceProject { get; set; }

        public string NameVersion => $"{Name} ({Version})";
    }
}
