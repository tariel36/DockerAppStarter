using System.Diagnostics;
using System.IO;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Core;
using DockerAppStarter.Gui.Extensions;
using DockerAppStarter.Gui.Internationalization;

namespace DockerAppStarter.Gui.Docker
{
    internal class DockerService
    {
        private const string ContainerStateRunning = "running";
        private const string StartupCommand = "docker";
        private const string DockerStartCommand = "start";

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
                try
                {
                    IList<ContainerListResponse>? res = await _dockerClient.Containers
                        .ListContainersAsync(
                            new()
                            {
                                All = true
                            },
                            cancellationToken);

                    return res.FirstOrDefault(
                               y =>
                                   y.Names.Any(z => z.Contains(containerName, StringComparison.InvariantCultureIgnoreCase))
                                   && y.State == ContainerStateRunning)
                           != null;
                }
                catch
                {
                    // Ignore
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
                try
                {
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = StartupCommand,
                        Arguments = string.Join(TextConstants.Space, args),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    };

                    using Process? _ = Process.Start(startInfo);
                }
                catch
                {
                    // Ignore
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }

        public string Combine(string? stackName, string serviceName)
        {
            return new [] { stackName, serviceName }
                .WhereNot(string.IsNullOrWhiteSpace)
                .Join(TextConstants.Hyphen);
        }
    }
}
