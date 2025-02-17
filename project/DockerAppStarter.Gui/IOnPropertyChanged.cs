using System.Runtime.CompilerServices;

namespace DockerAppStarter.Gui
{
    internal interface IOnPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string? propertyName = null);
    }
}
