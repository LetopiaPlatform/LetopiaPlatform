using LetopiaPlatform.Core.Common;
using Microsoft.AspNetCore.Http;

public interface IFileStorageService
{
    Task<Result<string>> UploadAsync(IFormFile file, string directory);
    Task<Result<List<string>>> UploadManyAsync(IEnumerable<IFormFile> files, string directory);
    Task<Result<string>> ReplaceAsync(IFormFile newFile, string directory, string oldFilePath);
    Task<Result> DeleteAsync(string filePath);
}
