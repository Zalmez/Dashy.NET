﻿using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.Models;
public class ItemEditModel
{
    public int Id { get; set; }
    public int SectionId { get; set; }

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

    public string? FeedUrl { get; set; } // RSS feed URL
    public int MaxItems { get; set; } = 5; // Maximum number of items to display
    public string? MaxSize { get; set; } // Maximum size of RSS items (e.g., rem, px, %)

    public Dictionary<string, object> Options { get; set; } = new();

    private Dictionary<string, object> OriginalOptions { get; set; } = new();

    public ItemEditModel() { }

    public ItemEditModel(ItemVm item)
    {
        Id = item.Id;
        SectionId = item.SectionId;
        Title = item.Title;
        Icon = item.Icon;
        Widget = item.Widget ?? "static-link";
        OriginalOptions = item.Options ?? new Dictionary<string, object>();
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
    }

    private string? GetOption(string key) =>
        OriginalOptions.TryGetValue(key, out var value) ? value?.ToString() : null;

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
        if (!string.IsNullOrWhiteSpace(MaxSize)) options["maxSize"] = MaxSize;
        return options;
    }

    public CreateItemDto ToCreateDto()
    {
        return new CreateItemDto(Title, Icon, Widget, SectionId, PackOptions());
    }

    public UpdateItemDto ToUpdateDto()
    {
        return new UpdateItemDto(Title, Icon, Widget, SectionId, PackOptions());
    }
}
