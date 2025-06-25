namespace DockerAppStarter.Gui.Docker
{
    internal class DockerCompose
    {
        public string? Name { get; init; }

        public List<DockerComposeService>? Services { get; init; }
    }
}
