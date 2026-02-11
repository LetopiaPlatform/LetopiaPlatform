using LetopiaPlatform.Core.Common;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.Services.Interfaces;

public interface IFileStorageService
{
    Task<Result<string>> UploadAsync(IFormFile file, string directory);    Task<Result<string>> ReplaceAsync(IFormFile newFile, string directory, string? oldFilePath);
    Task<Result> DeleteAsync(string filePath);
}