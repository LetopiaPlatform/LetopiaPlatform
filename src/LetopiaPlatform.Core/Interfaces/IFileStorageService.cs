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
    /// <returns>A result containing the relative file path on success.</returns>
    Task<Result<string>> UploadAsync(IFormFile file, string directory);

    /// <summary>
    /// Replaces an existing file with a new one, deleting the old file if it exists.
    /// </summary>
    /// <param name="newFile">The replacement file to upload.</param>
    /// <param name="directory">The target directory relative to the storage root.</param>
    /// <param name="oldFilePath">The relative path of the file to replace, or <c>null</c> if no previous file exists.</param>
    /// <returns>A result containing the new relative file path on success.</returns>
    Task<Result<string>> ReplaceAsync(IFormFile newFile, string directory, string? oldFilePath);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="filePath">The relative path of the file to delete.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(string filePath);
}