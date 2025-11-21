namespace Dashy.Net.Web.ViewModels;

public class SectionVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Dashy.Net.Shared.Models.ItemVm> Items { get; set; } = new();
}