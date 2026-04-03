namespace dashy3.Web.Models;

public static class WidgetType
{
    public const string Link = "link";
    public const string LinkGrid = "link-grid";
    public const string SystemStats = "system-stats";
    public const string Weather = "weather";
    public const string Markdown = "markdown";
    public const string Status = "status";
    public const string Media = "media";
    public const string Rss = "rss";
    public const string Cve = "cve";
    public const string Section = "section";
    public const string Api = "api";

    public static readonly IReadOnlyList<WidgetTypeInfo> All =
    [
        new(Link, "Link Shortcut", "link-icon", "Quick link to websites", ShowContainer: false),
        new(LinkGrid, "Link Grid", "grid-icon", "Grid of favourite links with favicons"),
        new(SystemStats, "System Stats", "activity-icon", "Monitor system performance"),
        new(Weather, "Weather", "cloud-icon", "Current weather conditions"),
        new(Markdown, "Markdown Notes", "file-text-icon", "Editable markdown content"),
        new(Status, "Status Checks", "check-circle-icon", "Service status monitoring"),
        new(Media, "Media Player", "music-icon", "Music and media controls"),
        new(Rss, "RSS Feed", "rss-icon", "Latest RSS feed items"),
        new(Cve, "CVE Tracker", "shield-icon", "Security vulnerability tracker"),
        new(Section, "Section Drawer", "folder-icon", "Collapsible container for other widgets"),
        new(Api, "API Widget", "code-icon", "Receive HTML/CSS content via API"),
    ];
}

public record WidgetTypeInfo(string Type, string Name, string Icon, string Description, bool ShowContainer = true);
