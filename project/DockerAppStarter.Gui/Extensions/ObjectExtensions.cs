namespace DockerAppStarter.Gui.Extensions
{
    internal static class ObjectExtensions
    {
        public static string OrFallback(this string? value, string fallbackValue)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallbackValue
                : value;
        }

        public static IEnumerable<TElement> OrEmpty<TElement>(this IEnumerable<TElement>? enumerable)
        {
            return enumerable ?? [ ];
        }

        public static string OrEmpty(this string? value)
        {
            return value.OrFallback(string.Empty);
        }

        public static IEnumerable<TElement> WhereNot<TElement>(this IEnumerable<TElement> enumerable, Func<TElement, bool> predicate)
        {
            return enumerable.Where(x => !predicate(x));
        }

        public static string Join<TElement>(this IEnumerable<TElement> enumerable, string separator)
        {
            return string.Join(separator, enumerable);
        }
    }
}
