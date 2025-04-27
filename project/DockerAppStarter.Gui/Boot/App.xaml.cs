using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Docker;
using DockerAppStarter.Gui.Internationalization;
using DockerAppStarter.Gui.ProcessStartup;
using DockerAppStarter.Gui.View.Services;
using Newtonsoft.Json;

namespace DockerAppStarter.Gui.Boot
{
    internal partial class App
    {
        private const string AppSettingsFilePath = "Assets/Configuration/appsettings.dev.json";

        private void App_OnStartup(object sender, StartupEventArgs args)
        {
            const string FileTag = "-f";

            string [] argv = args.Args;

            int fileIdx = Array.FindIndex(argv, static x => string.Equals(x, FileTag));

            DockerStartupContext.StartupConfiguration = fileIdx >= 0
                ? GetStartupConfigurationFromFile(TryGetNext(argv, fileIdx))
                : ParseArgs(argv);

            IReadOnlyDictionary<string, string> settings = GetSettings();

            DockerStartupContext.DockerExecutableFilePath = settings.GetValueOrDefault(nameof(DockerStartupContext.DockerExecutableFilePath));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? TryGetNext(string [] arr, int idx)
        {
            return idx + 1 < arr.Length
                ? arr[idx + 1]
                : null;
        }

        private static StartupConfiguration? GetStartupConfigurationFromFile(string? filePath)
        {
            if (File.Exists(filePath))
            {
                return JsonConvert.DeserializeObject<StartupConfiguration>(File.ReadAllText(filePath));
            }

            string errMessage = TranslationProvider.Instance.GetValueOrDefault(static () => Translations.FileNotFound, filePath);

            DialogService.ShowError(errMessage);

            throw new InvalidOperationException(errMessage);
        }

        private static IReadOnlyDictionary<string, string> GetSettings()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(AppSettingsFilePath))
                   ?? [ ];
        }

        private static StartupConfiguration ParseArgs(string [] argv)
        {
            const string ServiceTag = "-sv";
            const string ServiceDependenciesTag = "-d";
            const string StackTag = "-s";
            const string IconTag = "-i";

            List<string> dependencyNames = [ ];

            StartupConfiguration startupConfiguration = new();

            for (int i = 0; i < argv.Length; ++i)
            {
                switch (argv[i])
                {
                    case ServiceTag:
                    {
                        startupConfiguration = startupConfiguration with
                        {
                            Service = TryGetNext(argv, i)
                        };

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
                        startupConfiguration = startupConfiguration with
                        {
                            Stack = TryGetNext(argv, i)
                        };

                        break;
                    }

                    case IconTag:
                    {
                        startupConfiguration = startupConfiguration with
                        {
                            IconFilePath = TryGetNext(argv, i)
                        };

                        break;
                    }
                }
            }

            startupConfiguration = startupConfiguration with
            {
                Dependencies = dependencyNames.Select(
                        static x => new StartupConfiguration
                        {
                            Service = x
                        })
                    .ToList()
            };

            return startupConfiguration;
        }
    }
}
