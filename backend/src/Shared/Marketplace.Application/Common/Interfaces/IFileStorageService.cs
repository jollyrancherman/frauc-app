using Microsoft.AspNetCore.Http;
using Marketplace.Application.Common;

namespace Marketplace.Application.Common.Interfaces;

/// <summary>
/// File storage service for managing uploads and deletions
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload an image file to the specified container
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="containerName">The container/folder name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<Result<string>> UploadImageAsync(IFormFile file, string containerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an image file by URL
    /// </summary>
    /// <param name="imageUrl">The URL of the image to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a unique file name for the uploaded file
    /// </summary>
    /// <param name="originalFileName">The original file name</param>
    /// <returns>A unique file name</returns>
    string GenerateUniqueFileName(string originalFileName);
}