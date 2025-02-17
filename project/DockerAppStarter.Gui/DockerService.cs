// ReSharper disable MemberCanBeMadeStatic.Global

using System.Diagnostics;
using System.IO;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace DockerAppStarter.Gui
{
    internal class DockerService
    {
        private const string ContainerStateRunning = "running";

        private readonly DockerClient _dockerClient = new DockerClientConfiguration().CreateClient();

        public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return Task.FromResult(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(DockerStartupContext.DockerExecutableFilePath)).Length > 1);
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

                await Task.Delay(100, cancellationToken);
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

                    if (container == null)
                    {
                        return [ ];
                    }

                    return container.Ports.Select(static x => x.PublicPort).ToList();
                }
                catch
                {
                    // Ignore
                }

                await Task.Delay(100, cancellationToken);
            }

            return [ ];
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            string exeFilePath = DockerStartupContext.DockerExecutableFilePath.OrCallerThrow(TextConstants.DockerExecutableFilePathNotSet);

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
            string startupCommand = "docker";

            string [] args =
            [
                "start", containerName
            ];

            while (!await IsRunningAsync(containerName, cancellationToken))
            {
                try
                {
                    using Process _ = Process.Start(startupCommand, args);
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
            return $"{(string.IsNullOrWhiteSpace(stackName) ? string.Empty : $"{stackName}-")}{serviceName}";
        }
    }
}
