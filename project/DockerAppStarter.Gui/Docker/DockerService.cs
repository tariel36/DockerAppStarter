using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Core;
using DockerAppStarter.Gui.Extensions;
using DockerAppStarter.Gui.Internationalization;
using YamlDotNet.RepresentationModel;

namespace DockerAppStarter.Gui.Docker
{
    internal class DockerService
    {
        private const string ContainerStateRunning = "running";
        private const string DockerCommand = "docker";
        private const string DockerStartCommand = "start";
        private const string DockerComposeUpCommand = "compose -f {0} up -d";
        private const string DockerComposeDownCommand = "compose -f {0} down";

        private const int MinimumDockerProcesses = 1;

        private readonly DockerClient _dockerClient = new DockerClientConfiguration().CreateClient();

        public Task<bool> IsRunningAsync(CancellationToken _ = default)
        {
            try
            {
                return Task.FromResult(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(DockerStartupContext.DockerExecutableFilePath)).Length > MinimumDockerProcesses);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task<bool> IsRunningAsync(string containerName, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool? isRunning = await IsContainerRunning(containerName, cancellationToken);

                if (isRunning.HasValue)
                {
                    return isRunning.Value;
                }

                await Task.Delay(TimeConstants.Ms100, cancellationToken);
            }

            return false;
        }

        public async Task<bool> IsComposeRunningAsync(string composeFilePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(composeFilePath))
            {
                throw new FileNotFoundException(composeFilePath);
            }

            DockerCompose composeFile = LoadComposeFile(composeFilePath);

            string [] containers = composeFile.Services
                .OrEmpty()
                .WhereNot(static x => string.IsNullOrWhiteSpace(x.Name))
                .Select(static x => x.Name.OrEmpty())
                .ToArray();

            while (!cancellationToken.IsCancellationRequested)
            {
                bool? [] runningResults = await Task.WhenAll(containers.Select(x => IsContainerRunning(x, cancellationToken)).ToArray());

                if (runningResults.All(static x => x.HasValue && x.Value))
                {
                    return true;
                }

                if (runningResults.All(static x => x.HasValue && !x.Value))
                {
                    return false;
                }

                await Task.Delay(TimeConstants.Ms100, cancellationToken);
            }

            return false;
        }

        public async Task<IReadOnlyCollection<ushort>> GetPublicPorts(string containerName, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    IList<ContainerListResponse>? res = await _dockerClient.Containers
                        .ListContainersAsync(
                            new()
                            {
                                All = true
                            },
                            cancellationToken);

                    ContainerListResponse? container = res.FirstOrDefault(
                        y =>
                            y.Names.Any(z => z.Contains(containerName, StringComparison.InvariantCultureIgnoreCase))
                            && y.State == ContainerStateRunning);

                    return container == null
                        ? [ ]
                        : container.Ports.Select(static x => x.PublicPort).ToList();
                }
                catch
                {
                    // Ignore
                }

                await Task.Delay(TimeConstants.Ms100, cancellationToken);
            }

            return [ ];
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            string exeFilePath = DockerStartupContext.DockerExecutableFilePath.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DockerExecutableFilePathNotSet));

            if (await IsRunningAsync(cancellationToken))
            {
                return;
            }

            bool hasNotStarted = true;

            while (hasNotStarted)
            {
                try
                {
                    using Process _ = Process.Start(exeFilePath);
                    hasNotStarted = false;
                }
                catch
                {
                    // Ignore
                }
            }

            while (!await IsRunningAsync(cancellationToken))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
        }

        public async Task StartAsync(string containerName, CancellationToken cancellationToken = default)
        {
            string [] args =
            [
                DockerStartCommand, containerName
            ];

            while (!await IsRunningAsync(containerName, cancellationToken))
            {
                ExecuteProcess(string.Empty, args);

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }

        public async Task RestartComposeAsync(string composeFilePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(composeFilePath))
            {
                throw new FileNotFoundException(composeFilePath);
            }

            string composeDirectory = Path.GetDirectoryName(composeFilePath).OrCallerThrow();

            while (await IsComposeRunningAsync(composeFilePath, cancellationToken))
            {
                ExecuteProcess(composeDirectory, string.Format(DockerComposeDownCommand, composeFilePath));

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }

            while (!await IsComposeRunningAsync(composeFilePath, cancellationToken))
            {
                ExecuteProcess(composeDirectory, string.Format(DockerComposeUpCommand, composeFilePath));

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }

        public string Combine(string? stackName, string serviceName)
        {
            return new [] { stackName, serviceName }
                .WhereNot(string.IsNullOrWhiteSpace)
                .Join(TextConstants.Hyphen);
        }

        private async Task<bool?> IsContainerRunning(string containerName, CancellationToken cancellationToken)
        {
            try
            {
                IList<ContainerListResponse>? res = await _dockerClient.Containers
                    .ListContainersAsync(
                        new()
                        {
                            All = true
                        },
                        cancellationToken);

                ContainerListResponse? svc = res.FirstOrDefault(
                    y =>
                        y.Names.Any(z => z.Contains(containerName, StringComparison.InvariantCultureIgnoreCase))
                        && y.State == ContainerStateRunning);

                return svc != null;
            }
            catch
            {
                // Ignore
            }

            return null;
        }

        private static void ExecuteProcess(string workingDirectory, params string [] args)
        {
            try
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = DockerCommand,
                    Arguments = string.Join(TextConstants.Space, args),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                };

                using Process? p = Process.Start(startInfo);

                p?.WaitForExit();
            }
            catch
            {
                // Ignore
            }
        }

        private static DockerCompose LoadComposeFile(string filePath)
        {
            YamlStream yamlStream = new();
            yamlStream.Load(new StringReader(File.ReadAllText(filePath)));
            YamlMappingNode root = (YamlMappingNode) yamlStream.Documents[0].RootNode;
            YamlScalarNode name = (YamlScalarNode) root.Children[new YamlScalarNode("name")];
            YamlMappingNode services = (YamlMappingNode) root.Children[new YamlScalarNode("services")];

            return new()
            {
                Name = name.Value,
                Services = services.Children
                    .Select(
                        static x => new DockerComposeService
                        {
                            Name = ((YamlMappingNode) x.Value).Children[new YamlScalarNode("container_name")].ToString()
                        })
                    .ToList()
            };
        }
    }
}
