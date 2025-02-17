namespace DockerAppStarter.Gui
{
    internal static class ObjectExtensions
    {
        public static string OrFallback(this string? value, string fallbackValue)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallbackValue
                : value;
        }
    }
}
