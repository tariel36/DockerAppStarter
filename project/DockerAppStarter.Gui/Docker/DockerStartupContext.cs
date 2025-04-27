using DockerAppStarter.Gui.ProcessStartup;

namespace DockerAppStarter.Gui.Docker
{
    internal static class DockerStartupContext
    {
        public static StartupConfiguration? StartupConfiguration { get; set; }

        public static string? DockerExecutableFilePath { get; set; }
    }
}
