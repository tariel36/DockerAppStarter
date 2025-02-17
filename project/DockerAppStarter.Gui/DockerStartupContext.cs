namespace DockerAppStarter.Gui
{
    internal static class DockerStartupContext
    {
        public static string? StackName { get; set; }

        public static string? ServiceName { get; set; }

        public static IReadOnlyCollection<string> DependencyNames { get; set; } = new List<string>();

        public static string? IconFilePath { get; set; }

        public static string? DockerExecutableFilePath { get; set; }
    }
}
