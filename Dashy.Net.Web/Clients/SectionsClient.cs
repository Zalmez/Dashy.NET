using Dashy.Net.Shared.Models;
using System.Net.Http.Json;

namespace Dashy.Net.Web.Clients;

public class SectionsClient(HttpClient httpClient, ILogger<SectionsClient> logger)
{
    public async Task<bool> CreateAsync(CreateSectionDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/sections", dto);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to create section '{SectionName}'. Status: {StatusCode}", dto.Name, response.StatusCode);
                return false;
            }
            logger.LogInformation("Successfully created new section '{SectionName}'", dto.Name);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while creating section.");
            return false;
        }
    }
}