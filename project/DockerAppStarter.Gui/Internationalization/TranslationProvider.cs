using System.Globalization;
using System.Linq.Expressions;
using System.Resources;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Extensions;
using DockerAppStarter.Gui.Reflection;
using DockerAppStarter.Gui.ViewModel.Observability;

namespace DockerAppStarter.Gui.Internationalization
{
    internal class TranslationProvider
        : Observable
    {
        // TODO: Replace to Lock when .NET 9.0 becomes stable
        private static readonly object Lock = new();

        private static TranslationProvider? _instance;

        private readonly ResourceManager? _resourceManager = Translations.ResourceManager;

        private readonly CultureInfo? _currentCulture = Translations.Culture;

        internal static TranslationProvider Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                lock (Lock)
                {
                    _instance ??= new();
                }

                return _instance;
            }
        }

        public string GetValueOrDefault(string key)
        {
            return ResolveString(key);
        }

        public string GetValueOrDefault(Expression<Func<string>> keyProvider, Dictionary<string, string?> args)
        {
            return ResolveString(keyProvider.GetPropertyName(), args);
        }

        public string GetValueOrDefault(Expression<Func<string>> keyProvider)
        {
            return ResolveString(keyProvider.GetPropertyName());
        }

        public string GetValueOrDefault(Expression<Func<string>> keyProvider, params object? [] args)
        {
            return ResolveString(keyProvider.GetPropertyName(), args.Select(static (x, i) => KeyValuePair.Create(i.ToString(), x?.ToString())).ToDictionary());
        }

        private string ResolveString(string key, Dictionary<string, string?> args)
        {
            return args.Aggregate(
                GetValueOrDefault(key),
                static (prev, curr) => prev.Replace(BuildKey(curr.Key), curr.Value));
        }

        private string ResolveString(string key)
        {
            return _resourceManager.OrCallerThrow(Instance.GetValueOrDefault(static () => Translations.ResourceManagerIsNotSet))
                .GetString(key, _currentCulture)
                .OrFallback(CreateFallbackValue(key));
        }

        private static string CreateFallbackValue(string key)
        {
            return $"KEY:{key}";
        }

        private static string BuildKey(string key)
        {
            return $"{{{key}}}";
        }
    }
}
