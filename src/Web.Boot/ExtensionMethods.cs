using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Web.Boot
{
    internal static class ExtensionMethods
    {
        public static TEnum AsEnum<TEnum>(this IConfiguration configuration, string key, TEnum defaultValue)
            where TEnum : struct
        {
            return Enum.TryParse<TEnum>(configuration[key]?.Trim() ?? "", out var value) ? value : defaultValue;
        }

        public static string AsString(this IConfiguration configuration, string key, string defaultValue = "")
        {
            return string.IsNullOrWhiteSpace(configuration[key]?.Trim()) ? defaultValue : configuration[key]?.Trim();
        }

        public static int AsInt(this IConfiguration configuration, string key, int defaultValue = 0)
        {
            return int.TryParse(configuration[key]?.Trim() ?? "", out var value) ? value : defaultValue;
        }

        public static bool AsBool(this IConfiguration configuration, string key, bool defaultValue = false)
        {
            return bool.TryParse(configuration[key]?.Trim() ?? "", out var value) ? value : defaultValue;
        }

        public static string[] AsStringArray(this IConfiguration configuration, string key,
            char[] splitOnCharacters = null, string[] defaultValue = null)
        {
            var value = configuration[key]?.Trim();
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.SplitAndTrim(splitOnCharacters);
        }

        public static string[] SplitAndTrim(this string str, char[] splitOnCharacters = null)
        {
            if (string.IsNullOrWhiteSpace(str)) return Array.Empty<string>();

            return str.Split(splitOnCharacters ?? new[] {';', ',', ' '},
                StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }
    }
}