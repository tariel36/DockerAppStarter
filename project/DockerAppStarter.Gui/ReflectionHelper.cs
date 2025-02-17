using System.Linq.Expressions;

namespace DockerAppStarter.Gui
{
    internal static class ReflectionHelper
    {
        public static string GetPropertyName(this Expression<Func<string>> propertyExpression)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new InvalidOperationException(
                    string.Format(
                        TextConstants.InvalidMethodBody,
                        nameof(propertyExpression),
                        nameof(MemberExpression),
                        propertyExpression.Body.Type.Name));
            }

            string key = memberExpression.Member.Name;

            return key;
        }
    }
}
