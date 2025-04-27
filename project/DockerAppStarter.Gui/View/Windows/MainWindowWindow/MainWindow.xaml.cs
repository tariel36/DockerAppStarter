using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Core;
using DockerAppStarter.Gui.Docker;
using DockerAppStarter.Gui.Extensions;
using DockerAppStarter.Gui.Internationalization;
using DockerAppStarter.Gui.ProcessStartup;
using DockerAppStarter.Gui.ViewModel.Observability;

namespace DockerAppStarter.Gui.View.Windows.MainWindowWindow
{
    internal partial class MainWindow
        : INotifyPropertyChanged,
          IOnPropertyChanged
    {
        private const string DefaultIconFilePath = "Assets/Images/default.ico";
        private const string ExplorerExecutable = "explorer";
        private const string BaseHosting = "http://localhost";

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly DockerService _dockerService;
        private readonly StartupConfiguration _startupConfiguration;

        private string? _imagePath;

        public MainWindow()
        {
            _cancellationTokenSource = new();
            _dockerService = new();

            _startupConfiguration = GetStartupConfiguration();

            ImagePath = _startupConfiguration.IconFilePath;

            Title = _startupConfiguration.WindowTitle
                    .OrFallback(
                        _dockerService.Combine(
                            _startupConfiguration.Stack,
                            _startupConfiguration.Service.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DockerServiceNameNotSet))))
                ;

            Icon = ResolveIcon(_startupConfiguration);

            _ = Task.Factory.StartNew(IsAllComplete, _cancellationTokenSource.Token);

            LoadingSteps =
            [
                new(
                    TranslationProvider.Instance.GetValueOrDefault(static () => Translations.StartingDockerEllipsis),
                    DockerStarter,
                    _cancellationTokenSource.Token)
            ];

            foreach (StartProcessIndicatorViewModel svc in _startupConfiguration
                         .Dependencies
                         .OrEmpty()
                         .Select(
                             x => new StartProcessIndicatorViewModel(
                                 true,
                                 TranslationProvider.Instance.GetValueOrDefault(
                                     static () => Translations.Starting,
                                     new Dictionary<string, string?>
                                     {
                                         { TextConstants.Zero, x.DisplayName.OrFallback(x.Service.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DependencyServiceNotFound, x.Service))) }
                                     }),
                                 ctx => DockerServiceStarter(x, ctx),
                                 _cancellationTokenSource.Token)))
            {
                LoadingSteps.Add(svc);
            }

            LoadingSteps.Add(
                new(
                    TranslationProvider.Instance.GetValueOrDefault(
                        static () => Translations.Starting,
                        new Dictionary<string, string?>
                        {
                            { TextConstants.Zero, _startupConfiguration.DisplayName.OrFallback(_startupConfiguration.Service.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DockerServiceNameNotSet))) }
                        }),
                    DockerServiceStarter,
                    _cancellationTokenSource.Token));

            LoadingSteps.Add(
                new(
                    TranslationProvider.Instance.GetValueOrDefault(
                        static () => Translations.IsRunning,
                        new Dictionary<string, string?>
                        {
                            { TextConstants.Zero, _startupConfiguration.DisplayName.OrFallback(_startupConfiguration.Service.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DockerServiceNameNotSet))) }
                        }),
                    IsServiceRunning,
                    _cancellationTokenSource.Token));

            InitializeComponent();

            DataContext = this;
        }

        public ObservableCollection<StartProcessIndicatorViewModel> LoadingSteps { get; }

        public string? ImagePath
        {
            get { return _imagePath; }
            set { _ = Observable.Set(this, ref _imagePath, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        private async Task DockerStarter(StartProcessIndicatorCallbackContext ctx)
        {
            await _dockerService.StartAsync(ctx.CancellationToken);

            ctx.Complete(await _dockerService.IsRunningAsync(ctx.CancellationToken));
        }

        private async Task DockerServiceStarter(StartupConfiguration serviceConfig, StartProcessIndicatorCallbackContext ctx)
        {
            string containerFullName = _dockerService.Combine(
                serviceConfig.Stack,
                serviceConfig.Service.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DockerServiceNameNotSet)));

            await _dockerService.StartAsync(
                containerFullName,
                ctx.CancellationToken);

            ctx.Complete(await _dockerService.IsRunningAsync(containerFullName, ctx.CancellationToken));
        }

        private async Task IsServiceRunning(StartProcessIndicatorCallbackContext ctx)
        {
            string containerFullName = _dockerService.Combine(
                _startupConfiguration.Stack,
                _startupConfiguration.Service.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DockerServiceNameNotSet)));

            IReadOnlyCollection<ushort> publicPorts = await _dockerService.GetPublicPorts(containerFullName, ctx.CancellationToken);

            using HttpClient client = new();
            HttpResponseMessage? response = null;

            while (!ctx.CancellationToken.IsCancellationRequested || (response != null && response.IsSuccessStatusCode))
            {
                if (publicPorts.Count == 0)
                {
                    publicPorts = await _dockerService.GetPublicPorts(containerFullName, ctx.CancellationToken);
                }

                foreach (ushort port in publicPorts)
                {
                    try
                    {
                        response = await client.GetAsync($"{BaseHosting}{TextConstants.Colon}{port}");

                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }

                    await Task.Delay(TimeConstants.Ms1000, ctx.CancellationToken);
                }

                if (response?.IsSuccessStatusCode == true)
                {
                    break;
                }

                await Task.Delay(TimeConstants.Ms100, ctx.CancellationToken);
            }

            ctx.Complete(true);
        }

        private void Cancel_OnClick(object _, RoutedEventArgs __)
        {
            _cancellationTokenSource.Cancel();

            TryClose();
        }

        private async Task DockerServiceStarter(StartProcessIndicatorCallbackContext ctx)
        {
            if (_startupConfiguration.Dependencies.OrEmpty().Any())
            {
                List<StartProcessIndicatorViewModel> dependencies = LoadingSteps.Where(static x => x.IsDependency).ToList();

                while (dependencies.Any(static x => x.Status != true))
                {
                    await Task.Delay(TimeConstants.Ms1000, ctx.CancellationToken);
                }

                await Task.Delay(TimeConstants.Ms1000, ctx.CancellationToken);
            }

            await DockerServiceStarter(_startupConfiguration, ctx);
        }

        private async Task IsAllComplete()
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            string containerFullName = _dockerService.Combine(
                _startupConfiguration.Stack,
                _startupConfiguration.Service.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.DockerServiceNameNotSet)));

            IReadOnlyCollection<ushort> publicPorts = await _dockerService.GetPublicPorts(containerFullName, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (publicPorts.Count == 0)
                {
                    publicPorts = await _dockerService.GetPublicPorts(containerFullName, cancellationToken);
                }

                if (LoadingSteps.Count > 0 && LoadingSteps.All(static x => x.Status == true))
                {
                    foreach (ushort port in publicPorts)
                    {
                        try
                        {
                            using Process _ = Process.Start(ExplorerExecutable, $"{BaseHosting}{TextConstants.Colon}{port}");
                        }
                        catch
                        {
                            // Ignore
                        }

                        await Task.Delay(TimeConstants.Ms10, cancellationToken);
                    }

                    TryClose();
                }

                await Task.Delay(TimeConstants.Ms1000, cancellationToken);
            }
        }

        private void TryClose()
        {
            if (Dispatcher.CheckAccess())
            {
                Close();

                return;
            }

            Dispatcher.Invoke(TryClose);
        }

        private static ImageSource ResolveIcon(StartupConfiguration startupConfiguration)
        {
            Uri iconUri = string.IsNullOrWhiteSpace(startupConfiguration.IconFilePath)
                ? new(DefaultIconFilePath, UriKind.RelativeOrAbsolute)
                : new(startupConfiguration.IconFilePath, UriKind.RelativeOrAbsolute);

            return BitmapFrame.Create(iconUri);
        }

        private static StartupConfiguration GetStartupConfiguration()
        {
            return DockerStartupContext.StartupConfiguration.OrCallerThrow(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.StartupConfigurationIsNull));
        }
    }
}
