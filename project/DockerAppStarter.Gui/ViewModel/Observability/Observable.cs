using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DockerAppStarter.Gui.ViewModel.Observability
{
    internal class Observable
        : INotifyPropertyChanged,
          IOnPropertyChanged
    {
        protected Observable()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            return Set(this, ref field, value, propertyName);
        }

        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        public static bool Set<T>(IOnPropertyChanged source, ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName) || EqualityCheck(field, value))
            {
                return false;
            }

            field = value;

            source.OnPropertyChanged(propertyName);

            return true;
        }

        private static bool EqualityCheck<TValue>(TValue left, TValue right)
        {
            return EqualityComparer<TValue>.Default.Equals(left, right);
        }
    }
}
