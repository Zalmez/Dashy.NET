using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Dashy.Net.Shared.Models;

public class ItemEditModel
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public int? ParentItemId { get; set; } // New: optional container parent

    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = string.Empty;
    public string LinkTarget { get; set; } = "_self";
    public string? Icon { get; set; }
    public string Widget { get; set; } = "static-link";

    public string? Url { get; set; }
    public string? Description { get; set; }
    public string? LocationName { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string Unit { get; set; } = "celsius";

    public string? Timezone { get; set; }
    public string? TimeFormat { get; set; }

    public string? Provider { get; set; }
    public string? ApiKey { get; set; }

    public string? FeedUrl { get; set; }
    public int MaxItems { get; set; } = 5;
    public int RefreshInterval { get; set; } = 5; // Refresh interval in minutes
    public string? MaxSize { get; set; }

    public string? Severity { get; set; } // Added severity property for widgets like ThreatFeedWidget

    public Dictionary<string, object> Options { get; set; } = new();

    private JsonElement? OriginalOptions { get; set; }

    public ItemEditModel() { }

    public ItemEditModel(ItemVm item)
    {
        Id = item.Id;
        SectionId = item.SectionId;
        ParentItemId = item.ParentItemId; // New mapping
        Title = item.Title;
        Icon = item.Icon;
        Widget = item.Widget ?? "static-link";
        OriginalOptions = item.Options;
        LinkTarget = GetOption("target") ?? "_self";
        Url = GetOption("url");
        Description = GetOption("description");
        LocationName = GetOption("locationName");
        Latitude = GetOption("latitude");
        Longitude = GetOption("longitude");
        Unit = GetOption("unit") ?? "celsius";
        Timezone = GetOption("timezone") ?? "UTC";
        TimeFormat = GetOption("format") ?? "24-hour";
        Provider = GetOption("provider");
        ApiKey = GetOption("apiKey");
        FeedUrl = GetOption("feedUrl");
        MaxItems = int.TryParse(GetOption("maxItems"), out var maxItems) ? maxItems : 5;
        RefreshInterval = int.TryParse(GetOption("refreshInterval"), out var refreshInterval) ? refreshInterval : 5;
        Severity = GetOption("severity"); // Added severity initialization
    }

    private string? GetOption(string key)
    {
        if (OriginalOptions is not JsonElement element || element.ValueKind != JsonValueKind.Object)
            return null;

        if (TryGetPropertyIgnoreCase(element, key, out var prop))
        {
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => prop.ToString()
            };
        }
        return null;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private Dictionary<string, object> PackOptions()
    {
        var options = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(Url)) options["url"] = Url;
        if (!string.IsNullOrWhiteSpace(Description)) options["description"] = Description;
        if (!string.IsNullOrWhiteSpace(LocationName)) options["locationName"] = LocationName;
        if (!string.IsNullOrWhiteSpace(Latitude)) options["latitude"] = Latitude;
        if (!string.IsNullOrWhiteSpace(Longitude)) options["longitude"] = Longitude;
        if (!string.IsNullOrWhiteSpace(Unit)) options["unit"] = Unit;
        if (!string.IsNullOrWhiteSpace(LinkTarget)) options["target"] = LinkTarget;
        if (!string.IsNullOrWhiteSpace(Timezone)) options["timezone"] = Timezone;
        if (!string.IsNullOrWhiteSpace(TimeFormat)) options["format"] = TimeFormat;
        if (!string.IsNullOrWhiteSpace(Provider)) options["provider"] = Provider;
        if (!string.IsNullOrWhiteSpace(ApiKey)) options["apiKey"] = ApiKey;
        if (!string.IsNullOrWhiteSpace(FeedUrl)) options["feedUrl"] = FeedUrl;
        options["maxItems"] = MaxItems;
        options["refreshInterval"] = RefreshInterval;
        if (!string.IsNullOrWhiteSpace(MaxSize)) options["maxSize"] = MaxSize;
        if (!string.IsNullOrWhiteSpace(Severity)) options["severity"] = Severity; // Added severity packing
        return options;
    }

    public CreateItemDto ToCreateDto()
    {
        return new CreateItemDto(Title, Icon, Widget, SectionId, PackOptions(), ParentItemId);
    }

    public UpdateItemDto ToUpdateDto()
    {
        return new UpdateItemDto(Title, Icon, Widget, SectionId, PackOptions(), ParentItemId);
    }
}
