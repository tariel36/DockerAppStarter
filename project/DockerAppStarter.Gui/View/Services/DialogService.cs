using System.Windows;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Internationalization;

namespace DockerAppStarter.Gui.View.Services
{
    internal static class DialogService
    {
        public static void ShowError(string message)
        {
            _ = MessageBox.Show(message, TranslationProvider.Instance.GetValueOrDefault(static () => Translations.AnErrorOccurred));
        }
    }
}
