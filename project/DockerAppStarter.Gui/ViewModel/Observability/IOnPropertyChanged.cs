using System.Runtime.CompilerServices;

namespace DockerAppStarter.Gui.ViewModel.Observability
{
    internal interface IOnPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string? propertyName = null);
    }
}
