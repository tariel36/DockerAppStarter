namespace DockerAppStarter.Gui
{
    internal static class DockerStartupContext
    {
        public static StartupConfiguration? StartupConfiguration { get; set; }

        public static string? DockerExecutableFilePath { get; set; }
    }
}
