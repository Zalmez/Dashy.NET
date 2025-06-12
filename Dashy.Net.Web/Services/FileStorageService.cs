using Microsoft.AspNetCore.Components.Forms;

namespace Dashy.Net.Web.Services;

public class FileStorageService(
    IWebHostEnvironment webHostEnvironment,
    IConfiguration configuration,
    ILogger<FileStorageService> logger)
{
    public async Task<string?> SaveImageAsync(IBrowserFile file)
    {
        // max file size
        const int maxFileSizeMb = 5;
        const long maxFileSize = maxFileSizeMb * 1024 * 1024;
        if (file.Size > maxFileSize)
        {
            logger.LogWarning("File size exceeds {Limit}MB limit.", maxFileSize / 1024 / 1024);
            // TODO: Return a proper error response
            return null;
        }

        try
        {
            var customStoragePath = configuration["DASHYDOTNET_STORAGE_PATH"];
            string savePath;

            if (!string.IsNullOrWhiteSpace(customStoragePath))
            {
                savePath = customStoragePath;
            }
            else
            {
                var wwwRoot = webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
                savePath = Path.Combine(wwwRoot, "uploads");
            }

            Directory.CreateDirectory(savePath);

            var extension = Path.GetExtension(file.Name);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(savePath, uniqueFileName);

            await using var fs = new FileStream(filePath, FileMode.Create);
            await using var stream = file.OpenReadStream(maxFileSize);
            await stream.CopyToAsync(fs);

            var publicUrl = $"/uploads/{uniqueFileName}";
            logger.LogInformation("Successfully saved file to {FilePath}, accessible at {PublicUrl}", filePath, publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during file save operation.");
            return null;
        }
    }
}