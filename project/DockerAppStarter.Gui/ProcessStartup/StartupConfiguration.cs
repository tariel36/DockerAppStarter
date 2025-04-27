namespace DockerAppStarter.Gui.ProcessStartup
{
    internal record StartupConfiguration
    {
        public string? WindowTitle { get; init; }

        public string? DisplayName { get; init; }

        public string? Stack { get; init; }

        public string? Service { get; init; }

        public string? IconFilePath { get; init; }

        public string? ImageFilePath { get; init; }

        public IReadOnlyCollection<StartupConfiguration>? Dependencies { get; init; }
    }
}
