using System.Text;

namespace Dashy.Net.ApiService.Infrastructure;

internal static class LogSanitizer
{
    public static string Sanitize(object? value)
    {
        if (value is null) return string.Empty;
        var s = value.ToString();
        if (string.IsNullOrEmpty(s)) return s ?? string.Empty;

        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            switch (ch)
            {
                // Remove CR, LF, Unicode LS/PS, and other log-forging newlines entirely
                case '\r':
                case '\n':
                case '\u2028': // Line Separator
                case '\u2029': // Paragraph Separator
                    // skip (do not append)
                    break;
                default:
                    if (!char.IsControl(ch))
                        sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }
}
