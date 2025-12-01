using Bokra.Core.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Bokra.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;

        public FileStorageService(IWebHostEnvironment env, IHttpContextAccessor accessor)
        {
            _env = env;
            _accessor = accessor;
        }

        public async Task<Result<string>> UploadAsync(IFormFile file, string directory)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Result<string>.Failure("No file provided");

                var folderPath = Path.Combine(_env.WebRootPath, directory);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var fullPath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                var request = _accessor.HttpContext?.Request;
                var url = $"{request?.Scheme}://{request?.Host}/{directory}/{fileName}";

                return Result<string>.Success(url);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Upload failed: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> UploadManyAsync(IEnumerable<IFormFile> files, string directory)
        {
            try
            {
                if (files == null || !files.Any())
                    return Result<List<string>>.Failure("No files provided");

                var urls = new List<string>();
                foreach (var file in files)
                {
                    var result = await UploadAsync(file, directory);
                    if (!result.IsSuccess)
                        return Result<List<string>>.Failure(result.Errors);

                    urls.Add(result.Value!);
                }

                return Result<List<string>>.Success(urls);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.Failure($"Upload failed: {ex.Message}");
            }
        }

        public async Task<Result<string>> ReplaceAsync(IFormFile newFile, string directory, string oldFilePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(oldFilePath))
                    await DeleteAsync(oldFilePath);

                return await UploadAsync(newFile, directory);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Replace failed: {ex.Message}");
            }
        }

        public Task<Result> DeleteAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return Task.FromResult(Result.Failure("File path is empty"));

                var segments = filePath.Split('/').TakeLast(2).ToArray();
                if (segments.Length != 2)
                    return Task.FromResult(Result.Failure("Invalid file path"));

                var relativePath = Path.Combine(segments[0], segments[1]);
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(Result.Success());
                }

                return Task.FromResult(Result.Failure("File not found"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result.Failure($"Delete failed: {ex.Message}"));
            }
        }
    }
}
