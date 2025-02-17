using System.Runtime.CompilerServices;

namespace DockerAppStarter.Gui
{
    internal interface IOnPropertyChanging
    {
        void OnPropertyChanging([CallerMemberName] string? propertyName = null);
    }
}
