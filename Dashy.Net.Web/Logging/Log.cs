namespace Dashy.Net.Web.Logging;

public static class Log
{
    // Structured logging helpers (no source generator to avoid partial method build errors)
    public static void DashboardLoaded(ILogger logger, int sectionCount, int itemCount, long elapsedMs)
        => logger.LogInformation("Loaded dashboard with {SectionCount} sections and {ItemCount} items in {ElapsedMs}ms", sectionCount, itemCount, elapsedMs);

    public static void DashboardMissingConfig(ILogger logger)
        => logger.LogWarning("Dashboard config missing; user will see empty state");

    public static void ImageSaveFailed(ILogger logger, string fileName, string reason)
        => logger.LogError("Failed to save image {FileName}: {Reason}", fileName, reason);
}
