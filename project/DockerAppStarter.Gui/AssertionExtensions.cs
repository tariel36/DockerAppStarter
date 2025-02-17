using System.Runtime.CompilerServices;
using System.Text;

namespace DockerAppStarter.Gui
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

        public static TType OrThrow<TType>(this TType? obj, string? message = null)
            where TType : class
        {
            return obj ?? throw new NullReferenceException(message ?? TextConstants.InstanceOfClassIsNull);
        }

        public static TType OrThrow<TType>(this TType? obj, string? message = null)
            where TType : struct
        {
            return obj ?? throw new NullReferenceException(message ?? TextConstants.InstanceOfStructIsNull);
        }

        private static string CreateMessage(string? caller, string? filePath, int? lineNbr)
        {
            StringBuilder sb = new(TextConstants.InstanceOfClassIsNull);

            if (!string.IsNullOrWhiteSpace(caller))
            {
                _ = sb.AppendFormat(TextConstants.CalledBy, caller);
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _ = sb.AppendFormat(TextConstants.InFile, filePath);
            }

            if (lineNbr.HasValue)
            {
                _ = sb.AppendFormat(TextConstants.AtLine, lineNbr.Value);
            }

            _ = sb.Append(TextConstants.Period);

            return sb.ToString();
        }
    }
}
