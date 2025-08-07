using System.ComponentModel.DataAnnotations;

namespace Dashy.Net.Shared.Models;

public class AppSettings
{
    public int Id { get; set; }
    public string? CustomLogoPath { get; set; }
}
