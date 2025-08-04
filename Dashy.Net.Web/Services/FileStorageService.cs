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
        const int maxFileSizeMb = 10;
        const long maxFileSize = maxFileSizeMb * 1024 * 1024;
        
        logger.LogInformation("Starting image save for file: {FileName}, Size: {Size} bytes, ContentType: {ContentType}", 
            file.Name, file.Size, file.ContentType);
        
        if (file.Size > maxFileSize)
        {
            logger.LogWarning("File size {FileSize} exceeds {Limit}MB limit.", file.Size, maxFileSizeMb);
            return null;
        }

        if (!await IsSupportedImageFormatAsync(file))
        {
            logger.LogWarning("File '{FileName}' rejected: not a supported image format.", file.Name);
            return null;
        }

        try
        {
            var customStoragePath = configuration["DASHYDOTNET_STORAGE_PATH"];
            string savePath;
            bool isCustomStorage = !string.IsNullOrWhiteSpace(customStoragePath);

            if (isCustomStorage)
            {
                savePath = customStoragePath!;
                logger.LogInformation("Using custom storage path: {StoragePath}", savePath);
            }
            else
            {
                var wwwRoot = webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
                savePath = Path.Combine(wwwRoot, "uploads");
                logger.LogInformation("Using default storage path: {StoragePath}", savePath);
            }

            // Ensure the directory exists
            if (!Directory.Exists(savePath))
            {
                logger.LogInformation("Creating directory: {DirectoryPath}", savePath);
                Directory.CreateDirectory(savePath);
            }
            else
            {
                logger.LogInformation("Directory already exists: {DirectoryPath}", savePath);
            }

            var extension = Path.GetExtension(file.Name);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(savePath, uniqueFileName);

            logger.LogInformation("Attempting to save file to: {FilePath}", filePath);

            await using var fs = new FileStream(filePath, FileMode.Create);
            await using var stream = file.OpenReadStream(maxFileSize);
            await stream.CopyToAsync(fs);

            // Verify the file was actually saved
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                logger.LogInformation("File successfully saved. Size: {FileSize} bytes, Path: {FilePath}", fileInfo.Length, filePath);
            }
            else
            {
                logger.LogError("File was not found after save operation: {FilePath}", filePath);
                return null;
            }

            var publicUrl = $"/uploads/{uniqueFileName}";
            logger.LogInformation("File accessible at public URL: {PublicUrl}", publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during file save operation.");
            return null;
        }
    }

    private async Task<bool> IsSupportedImageFormatAsync(IBrowserFile file)
    {
        byte[] header = ArrayPool<byte>.Shared.Rent(256);
        try
        {
            // Use the full file size limit here, we only read the first 256 bytes
            await using var stream = file.OpenReadStream(10 * 1024 * 1024);
            int read = await stream.ReadAsync(header, 0, 256); // Only read first 256 bytes
            
            logger.LogInformation("File: {FileName}, ContentType: {ContentType}, Size: {Size}, BytesRead: {BytesRead}", 
                file.Name, file.ContentType, file.Size, read);
            
            if (read >= 8)
            {
                var headerHex = Convert.ToHexString(header, 0, Math.Min(read, 16));
                logger.LogInformation("First 16 bytes (hex): {HeaderHex}", headerHex);
            }
            
            if (read < 4) 
            {
                logger.LogWarning("File too small, only {BytesRead} bytes read", read);
                return false;
            }

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                logger.LogInformation("Detected JPEG format");
                return true;
            }
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                logger.LogInformation("Detected PNG format");
                return true;
            }
            // GIF: GIF87a or GIF89a
            if (read >= 6 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && 
                header[3] == 0x38 && (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61)
            {
                logger.LogInformation("Detected GIF format");
                return true;
            }
            // BMP: BM
            if (header[0] == 0x42 && header[1] == 0x4D)
            {
                logger.LogInformation("Detected BMP format");
                return true;
            }
            var text = Encoding.UTF8.GetString(header, 0, read).TrimStart();
            if (text.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) || text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && text.Contains("<svg", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Detected SVG format");
                return true;
            }
            
            logger.LogWarning("Unknown image format detected");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking image format for file {FileName}", file.Name);
            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(header);
        }
    }
}