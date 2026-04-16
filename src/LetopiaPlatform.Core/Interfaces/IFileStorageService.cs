using LetopiaPlatform.Core.Common;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.Services.Interfaces;

/// <summary>
/// Manages file upload, replacement, and deletion for user-generated content.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to the specified directory.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="directory">The target directory relative to the storage root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the relative file path on success.</returns>
    Task<Result<string>> UploadAsync(IFormFile file, string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing file with a new one, deleting the old file if it exists.
    /// </summary>
    /// <param name="newFile">The replacement file to upload.</param>
    /// <param name="directory">The target directory relative to the storage root.</param>
    /// <param name="oldFilePath">The relative path of the file to replace, or <c>null</c> if no previous file exists.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the new relative file path on success.</returns>
    Task<Result<string>> ReplaceAsync(IFormFile newFile, string directory, string? oldFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="filePath">The relative path of the file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads an SVG file to the specified directory with size validation.
    /// </summary>
    /// <param name="file">The SVG file to upload.</param>
    /// <param name="directory">The target directory relative to the storage root.</param>
    /// <param name="maxSizeBytes">The maximum allowed file size in bytes (default: 256 KB).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the relative file path on success.</returns>
    Task<Result<string>> UploadSvgAsync(
        IFormFile file,
        string directory,
        long maxSizeBytes = 256 * 1024,
        CancellationToken ct = default);
}