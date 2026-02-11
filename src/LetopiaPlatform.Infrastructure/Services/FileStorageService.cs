using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;

        // Allowed file extensions whitelist
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"
        };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

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

                if (file.Length > MaxFileSizeBytes)
                    return Result<string>.Failure("File exceeds maximum size of 5 MB");

                var extension = Path.GetExtension(file.FileName);
                if (!AllowedExtensions.Contains(extension))
                    return Result<string>.Failure($"File type '{extension}' is not allowed");

                var folderPath = GetSafeFolderPath(directory);
                if (folderPath == null)
                    return Result<string>.Failure("Invalid directory");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
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
        
        public async Task<Result<string>> ReplaceAsync(IFormFile newFile, string directory, string? oldFilePath)
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

                var uri = new Uri(filePath, UriKind.RelativeOrAbsolute);
                var segments = uri.IsAbsoluteUri
                    ? uri.AbsolutePath.TrimStart('/').Split('/')
                    : filePath.Split('/');

                if (segments.Length < 2)
                    return Task.FromResult(Result.Failure("Invalid file path"));

                var relativePath = Path.Combine(segments[^2], segments[^1]);
                var fullPath = Path.GetFullPath(Path.Combine(_env.WebRootPath, relativePath));

                // Path traversal guard
                if (!fullPath.StartsWith(_env.WebRootPath, StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(Result.Failure("Invalid file path"));

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

        /// <summary>
        /// Resolves directory to a safe path within wwwroot. Returns null if path escapes wwwroot.
        /// </summary>
        private string? GetSafeFolderPath(string directory)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_env.WebRootPath, directory));
            return fullPath.StartsWith(_env.WebRootPath, StringComparison.OrdinalIgnoreCase)
                ? fullPath
                : null;
        }
    }
}