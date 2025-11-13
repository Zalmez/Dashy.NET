namespace Dashy.Net.Web.ViewModels;

public class Theme
{
    public string Name { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    // Optional per-theme stylesheet to override tokens/variables
    public string? CssHref { get; set; }
}

