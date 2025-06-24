using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.Models;
public class ItemEditModel
{
    public int Id { get; set; }
    public int SectionId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = string.Empty;

    public string? Icon { get; set; }
    public string Widget { get; set; } = "static-link";

    public string? Url { get; set; }
    public string? Description { get; set; }
    public string? LocationName { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string Unit { get; set; } = "celsius";

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

        Url = GetOption("url");
        Description = GetOption("description");
        LocationName = GetOption("locationName");
        Latitude = GetOption("latitude");
        Longitude = GetOption("longitude");
        Unit = GetOption("unit") ?? "celsius";
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
