// ReSharper disable MemberCanBePrivate.Global

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DockerAppStarter.Gui
{
    internal class Observable
        : INotifyPropertyChanged,
          INotifyPropertyChanging,
          IOnPropertyChanged,
          IOnPropertyChanging
    {
        protected Observable()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event PropertyChangingEventHandler? PropertyChanging;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            return Set(this, ref field, value, propertyName);
        }

        protected bool Set<T>(Func<T>? getter, Action<T>? setter, T value, [CallerMemberName] string? propertyName = null)
        {
            return Set(this, getter, setter, value, propertyName);
        }

        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        public virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null)
        {
            PropertyChanging?.Invoke(this, new(propertyName));
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

        public static bool Set<T>(IOnPropertyChanged source, Func<T>? getter, Action<T>? setter, T value, [CallerMemberName] string? propertyName = null)
        {
            if (getter == null || setter == null || string.IsNullOrWhiteSpace(propertyName) || EqualityCheck(getter(), value))
            {
                return false;
            }

            setter(value);

            source.OnPropertyChanged(propertyName);

            return true;
        }

        private static bool EqualityCheck<TValue>(TValue left, TValue right)
        {
            return EqualityComparer<TValue>.Default.Equals(left, right);
        }
    }
}
