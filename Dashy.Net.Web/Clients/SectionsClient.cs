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

    public async Task<bool> UpdateAsync(int sectionId, UpdateSectionDto dto)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/sections/{sectionId}", dto);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to update section {SectionId}. Status: {StatusCode}", sectionId, response.StatusCode);
                return false;
            }
            logger.LogInformation("Successfully updated section {SectionId}", sectionId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while updating section {SectionId}", sectionId);
            return false;
        }
    }

    public async Task<bool> ReorderAsync(ReorderSectionsDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/sections/reorder", dto);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to reorder sections. Status: {StatusCode}", response.StatusCode);
                return false;
            }
            logger.LogInformation("Successfully reordered sections.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while reordering sections.");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int sectionId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/sections/{sectionId}");
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to delete section {SectionId}. Status: {StatusCode}", sectionId, response.StatusCode);
                return false;
            }
            logger.LogInformation("Successfully deleted section {SectionId}", sectionId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while deleting section {SectionId}", sectionId);
            return false;
        }
    }

}