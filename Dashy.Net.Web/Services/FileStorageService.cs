using Microsoft.AspNetCore.Components.Forms;
using System.Buffers;
using System.Text;

namespace Dashy.Net.Web.Services;

public class FileStorageService(
    IWebHostEnvironment webHostEnvironment,
    IConfiguration configuration,
    ILogger<FileStorageService> logger)
{
    public async Task<string?> SaveImageAsync(IBrowserFile file)
    {
        const int maxFileSizeMb = 5;
        const long maxFileSize = maxFileSizeMb * 1024 * 1024;
        if (file.Size > maxFileSize)
        {
            logger.LogWarning("File size exceeds {Limit}MB limit.", maxFileSize / 1024 / 1024);
            return null;
        }

        if (!await IsSupportedImageFormatAsync(file))
        {
            logger.LogWarning("File rejected: not a supported image format.");
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

    private static async Task<bool> IsSupportedImageFormatAsync(IBrowserFile file)
    {
        byte[] header = ArrayPool<byte>.Shared.Rent(256);
        try
        {
            await using var stream = file.OpenReadStream(256);
            int read = await stream.ReadAsync(header, 0, 256);
            if (read < 4) return false;

            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return true;
            if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                return true;
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                return true;
            if (header[0] == 0x42 && header[1] == 0x4D)
                return true;
            var text = Encoding.UTF8.GetString(header, 0, read).TrimStart();
            if (text.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) || text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && text.Contains("<svg", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(header);
        }
    }
}