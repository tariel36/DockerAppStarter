using System.Linq.Expressions;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Internationalization;

namespace DockerAppStarter.Gui.Reflection
{
    internal static class ReflectionHelper
    {
        public static string GetPropertyName(this Expression<Func<string>> propertyExpression)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new InvalidOperationException(
                    TranslationProvider.Instance.GetValueOrDefault(
                        static () => Translations.InvalidMethodBody,
                        nameof(propertyExpression),
                        nameof(MemberExpression),
                        propertyExpression.Body.Type.Name));
            }

            string key = memberExpression.Member.Name;

            return key;
        }
    }
}
