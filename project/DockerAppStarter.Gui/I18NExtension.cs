// ReSharper disable MemberCanBePrivate.Global

using System.Reflection;
using System.Windows.Data;

namespace DockerAppStarter.Gui
{
    internal class I18NExtension
        : Binding
    {
        public I18NExtension(string propertyKey, object dataContext)
            : base(CreateKey(ExtractKey(propertyKey, dataContext)))
        {
            Key = ExtractKey(propertyKey, dataContext);
            Provider = TranslationProvider.Instance;

            Mode = BindingMode.OneWay;
            Source = Provider;
        }

        public I18NExtension(string key)
            : base(CreateKey(key))
        {
            Key = key;
            Provider = TranslationProvider.Instance;

            Mode = BindingMode.OneWay;
            Source = Provider;
        }

        public string Key { get; }

        public TranslationProvider Provider { get; }

        public override string ToString()
        {
            return Provider.GetValueOrDefault(Key);
        }

        private static string CreateKey(string key)
        {
            return $"[{key}]";
        }

        private static string ExtractKey(string propertyKey, object dataContext)
        {
            return dataContext.GetType()
                       .GetProperty(propertyKey, BindingFlags.Public | BindingFlags.Instance)
                       ?.GetValue(dataContext)
                       ?.ToString()
                   ?? propertyKey;
        }
    }
}
