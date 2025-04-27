using System.Runtime.CompilerServices;
using System.Text;
using DockerAppStarter.Gui.Assets.I18N;
using DockerAppStarter.Gui.Core;
using DockerAppStarter.Gui.Internationalization;

namespace DockerAppStarter.Gui.Extensions
{
    internal static class AssertionExtensions
    {
        public static TType OrCallerThrow<TType>(
            this TType? obj,
            string? message = null,
            [CallerMemberName] string? caller = default,
            [CallerFilePath] string? filepath = null,
            [CallerLineNumber] int? lineNbr = null)
            where TType : class
        {
            return obj ?? throw new NullReferenceException(message ?? CreateMessage(caller, filepath, lineNbr));
        }

        public static TType OrCallerThrow<TType>(
            this TType? obj,
            string? message = null,
            [CallerMemberName] string? caller = default,
            [CallerFilePath] string? filepath = null,
            [CallerLineNumber] int? lineNbr = null)
            where TType : struct
        {
            return obj ?? throw new NullReferenceException(message ?? CreateMessage(caller, filepath, lineNbr));
        }

        private static string CreateMessage(string? caller, string? filePath, int? lineNbr)
        {
            StringBuilder sb = new(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.InstanceOfClassIsNull));

            if (!string.IsNullOrWhiteSpace(caller))
            {
                _ = sb.Append(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.CalledBy, caller));
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _ = sb.Append(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.InFile, filePath));
            }

            if (lineNbr.HasValue)
            {
                _ = sb.Append(TranslationProvider.Instance.GetValueOrDefault(static () => Translations.AtLine, lineNbr.Value));
            }

            _ = sb.Append(TextConstants.Period);

            return sb.ToString();
        }
    }
}
