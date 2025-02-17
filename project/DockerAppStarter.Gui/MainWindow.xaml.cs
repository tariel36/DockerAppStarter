using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DockerAppStarter.Gui
{
    internal partial class MainWindow
        : INotifyPropertyChanged,
          IOnPropertyChanged
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        private string? _imagePath;

        public MainWindow()
        {
            _cancellationTokenSource = new();

            ImagePath = DockerStartupContext.IconFilePath;

            Title = new DockerService().Combine(
                DockerStartupContext.StackName,
                DockerStartupContext.ServiceName.OrCallerThrow(TextConstants.DockerServiceNameNotSet));

            _ = Task.Factory.StartNew(IsAllComplete, _cancellationTokenSource.Token);

            LoadingSteps =
            [
                new(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.StartingDockerEllipsis), DockerStarter, _cancellationTokenSource.Token),
                new(
                    TranslationProvider.Instance.GetValueOrDefault(
                        static () => Translations.Starting_0_,
                        new()
                        {
                            { TextConstants.Zero, DockerStartupContext.ServiceName.OrCallerThrow(TextConstants.DockerServiceNameNotSet) }
                        }),
                    DockerServiceStarter,
                    _cancellationTokenSource.Token),
                new(
                    TranslationProvider.Instance.GetValueOrDefault(
                        static () => Translations.Is_0_Running,
                        new()
                        {
                            { TextConstants.Zero, DockerStartupContext.ServiceName.OrCallerThrow(TextConstants.DockerServiceNameNotSet) }
                        }),
                    IsServiceRunning,
                    _cancellationTokenSource.Token)
            ];

            InitializeComponent();

            DataContext = this;
        }

        public ObservableCollection<StartProcessIndicatorViewModel> LoadingSteps
        {
            get;
        }

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

        private static async Task DockerStarter(StartProcessIndicatorCallbackContext ctx)
        {
            DockerService docker = new();

            await docker.StartAsync(ctx.CancellationToken);

            ctx.Complete(await docker.IsRunningAsync(ctx.CancellationToken));
        }

        private static async Task DockerServiceStarter(StartProcessIndicatorCallbackContext ctx)
        {
            DockerService docker = new();

            string containerFullName = docker.Combine(
                DockerStartupContext.StackName,
                DockerStartupContext.ServiceName.OrCallerThrow(TextConstants.DockerServiceNameNotSet));

            await docker.StartAsync(
                containerFullName,
                ctx.CancellationToken);

            ctx.Complete(await docker.IsRunningAsync(containerFullName, ctx.CancellationToken));
        }

        private static async Task IsServiceRunning(StartProcessIndicatorCallbackContext ctx)
        {
            DockerService docker = new();

            string containerFullName = docker.Combine(
                DockerStartupContext.StackName,
                DockerStartupContext.ServiceName.OrCallerThrow(TextConstants.DockerServiceNameNotSet));

            IReadOnlyCollection<ushort> publicPorts = await docker.GetPublicPorts(containerFullName, ctx.CancellationToken);

            using HttpClient client = new();
            HttpResponseMessage? response = null;

            while (!ctx.CancellationToken.IsCancellationRequested || (response != null && response.IsSuccessStatusCode))
            {
                if (publicPorts.Count == 0)
                {
                    publicPorts = await docker.GetPublicPorts(containerFullName, ctx.CancellationToken);
                }

                foreach (ushort port in publicPorts)
                {
                    try
                    {
                        response = await client.GetAsync($"http://localhost:{port}");

                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(100), ctx.CancellationToken);
                }

                if (response?.IsSuccessStatusCode == true)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000), ctx.CancellationToken);
            }

            ctx.Complete(true);
        }

        private void Cancel_OnClick(object _, RoutedEventArgs __)
        {
            _cancellationTokenSource.Cancel();

            TryClose();
        }

        private async Task IsAllComplete()
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            DockerService docker = new();

            string containerFullName = docker.Combine(
                DockerStartupContext.StackName,
                DockerStartupContext.ServiceName.OrCallerThrow(TextConstants.DockerServiceNameNotSet));

            IReadOnlyCollection<ushort> publicPorts = await docker.GetPublicPorts(containerFullName, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (publicPorts.Count == 0)
                {
                    publicPorts = await docker.GetPublicPorts(containerFullName, cancellationToken);
                }

                if (LoadingSteps.Count > 0 && LoadingSteps.All(static x => x.Status == true))
                {
                    foreach (ushort port in publicPorts)
                    {
                        try
                        {
                            using Process _ = Process.Start("explorer", $"http://localhost:{port}");
                        }
                        catch
                        {
                            // Ignore
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
                    }

                    TryClose();
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
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
    }
}
