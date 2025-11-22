using Dashy.Net.Shared.Models;
using System.Net.Http.Json;

namespace Dashy.Net.Web.Clients;

public class ItemsClient(HttpClient httpClient, ILogger<ItemsClient> logger)
{
    public async Task<bool> CreateAsync(CreateItemDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/items", dto);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to create item '{ItemTitle}'. Status: {StatusCode}. Body: {Body}", dto.Title, response.StatusCode, body);
                return false;
            }
            logger.LogInformation("Successfully created new item '{ItemTitle}'", dto.Title);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while creating item.");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int itemId, UpdateItemDto dto)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/items/{itemId}", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to update item {ItemId}. Status: {StatusCode}, Reason: {Reason}",
                    itemId, response.StatusCode, errorContent);
                return false;
            }
            logger.LogInformation("Successfully updated item {ItemId}", itemId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while updating item {ItemId}", itemId);
            return false;
        }
    }

    public async Task<bool> ReorderAsync(ReorderItemsDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/items/reorder", dto);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to reorder items. Status: {StatusCode}", response.StatusCode);
                return false;
            }
            logger.LogInformation("Successfully reordered items.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while reordering items.");
            return false;
        }
    }

    public async Task<bool> MoveAsync(MoveItemDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/items/move", dto);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to move item {ItemId}. Status: {StatusCode}. Body: {Body}", dto.ItemId, response.StatusCode, body);
                return false;
            }
            logger.LogInformation("Successfully moved item {ItemId} to section {SectionId} parent {ParentItemId} position {Position}", dto.ItemId, dto.NewSectionId, dto.NewParentItemId, dto.NewPosition);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while moving item {ItemId}", dto.ItemId);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int itemId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/items/{itemId}");
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to delete item {ItemId}. Status: {StatusCode}", itemId, response.StatusCode);
                return false;
            }
            logger.LogInformation("Successfully deleted item {ItemId}", itemId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while deleting item {ItemId}", itemId);
            return false;
        }
    }

    public async Task<bool> ReorderScopedAsync(ReorderItemsScopedDto dto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/items/reorder/scoped", dto);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed scoped reorder for Section {SectionId} Parent {ParentItemId}. Status: {StatusCode}", dto.SectionId, dto.ParentItemId, response.StatusCode);
                return false;
            }
            logger.LogInformation("Scoped reorder success Section {SectionId} Parent {ParentItemId}", dto.SectionId, dto.ParentItemId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during scoped reorder Section {SectionId} Parent {ParentItemId}", dto.SectionId, dto.ParentItemId);
            return false;
        }
    }
}

public class MoveItemDto
{
    public int ItemId { get; set; }
    public int? NewSectionId { get; set; }
    public int? NewParentItemId { get; set; }
    public bool ClearParent { get; set; }
    public int? NewPosition { get; set; }
}