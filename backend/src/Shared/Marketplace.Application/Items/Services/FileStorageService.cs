using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Application.Common;

namespace Marketplace.Application.Items.Services;

/// <summary>
/// Local file storage service for development and testing
/// In production, this should be replaced with cloud storage (Azure Blob, AWS S3, etc.)
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _baseStoragePath;
    private readonly string _baseUrl;
    
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // For development - in production this should come from configuration
        _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = "https://localhost:7001/uploads"; // Base URL for serving images
        
        EnsureStorageDirectoryExists();
    }

    public async Task<Result<string>> UploadImageAsync(IFormFile file, string containerName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate container name
            if (string.IsNullOrWhiteSpace(containerName))
            {
                return Result.Failure<string>("Container name cannot be empty");
            }

            // Validate file
            var validationResult = ValidateImageFile(file);
            if (!validationResult.IsSuccess)
            {
                return Result.Failure<string>(validationResult.ErrorMessage!);
            }

            // Generate unique file name
            var uniqueFileName = GenerateUniqueFileName(file.FileName);
            
            // Create container directory if it doesn't exist
            var containerPath = Path.Combine(_baseStoragePath, containerName);
            Directory.CreateDirectory(containerPath);
            
            // Full file path
            var filePath = Path.Combine(containerPath, uniqueFileName);
            
            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Return public URL
            var publicUrl = $"{_baseUrl}/{containerName}/{uniqueFileName}";
            
            _logger.LogInformation("Successfully uploaded image: {FileName} to {Url}", file.FileName, publicUrl);
            
            return Result.Success(publicUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image: {FileName}", file.FileName);
            return Result.Failure<string>($"Failed to upload image: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return Result.Failure<bool>("Image URL cannot be empty");
            }

            // Extract file path from URL
            if (!imageUrl.StartsWith(_baseUrl))
            {
                _logger.LogWarning("Invalid image URL format: {Url}", imageUrl);
                return Result.Failure<bool>("Invalid image URL format");
            }

            var relativePath = imageUrl.Substring(_baseUrl.Length).TrimStart('/');
            var filePath = Path.Combine(_baseStoragePath, relativePath);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath), cancellationToken);
                _logger.LogInformation("Successfully deleted image: {Url}", imageUrl);
            }
            else
            {
                _logger.LogWarning("Image file not found: {Path}", filePath);
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {Url}", imageUrl);
            return Result.Failure<bool>($"Failed to delete image: {ex.Message}");
        }
    }

    public string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters
        
        return $"{timestamp}_{guid}{extension}";
    }

    private static Result<bool> ValidateImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Result.Failure<bool>("File cannot be empty");
        }

        // Check file size
        if (file.Length > MaxFileSizeBytes)
        {
            return Result.Failure<bool>($"File size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB");
        }

        // Check content type
        if (!AllowedImageTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return Result.Failure<bool>($"Invalid file type. Allowed types: {string.Join(", ", AllowedImageTypes)}");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return Result.Failure<bool>($"Invalid file extension. Allowed extensions: {string.Join(", ", AllowedExtensions)}");
        }

        return Result.Success(true);
    }

    private void EnsureStorageDirectoryExists()
    {
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
            _logger.LogInformation("Created storage directory: {Path}", _baseStoragePath);
        }
    }
}