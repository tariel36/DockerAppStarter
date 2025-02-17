// ReSharper disable MemberCanBeFileLocal

using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Newtonsoft.Json;

namespace DockerAppStarter.Gui
{
    internal partial class App
    {
        private const string AppSettingsFilePath = "appsettings.dev.json";

        private void App_OnStartup(object sender, StartupEventArgs args)
        {
            const string ServiceTag = "-sv";
            const string ServiceDependenciesTag = "-d";
            const string StackTag = "-s";
            const string IconTag = "-i";

            string [] argv = args.Args;

            List<string> dependencyNames = new();

            for (int i = 0; i < argv.Length; ++i)
            {
                switch (argv[i])
                {
                    case ServiceTag:
                    {
                        DockerStartupContext.ServiceName = TryGetNext(argv, i);

                        break;
                    }

                    case ServiceDependenciesTag:
                    {
                        string? serviceName = TryGetNext(argv, i);

                        if (string.IsNullOrWhiteSpace(serviceName))
                        {
                            break;
                        }

                        dependencyNames.Add(serviceName);

                        break;
                    }

                    case StackTag:
                    {
                        DockerStartupContext.StackName = TryGetNext(argv, i);

                        break;
                    }

                    case IconTag:
                    {
                        DockerStartupContext.IconFilePath = TryGetNext(argv, i);

                        break;
                    }
                }
            }

            DockerStartupContext.DependencyNames = dependencyNames;

            IReadOnlyDictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(AppSettingsFilePath))
                                                           ?? new();

            DockerStartupContext.DockerExecutableFilePath = settings.GetValueOrDefault(nameof(DockerStartupContext.DockerExecutableFilePath));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? TryGetNext(string [] arr, int idx)
        {
            return idx + 1 < arr.Length
                ? arr[idx + 1]
                : null;
        }
    }
}
