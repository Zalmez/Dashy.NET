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
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\n':
                    sb.Append("\\n");
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
