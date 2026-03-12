using Amazon.S3;
using Amazon.S3.Model;
using LetopiaPlatform.Core.AppSettings;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetopiaPlatform.Infrastructure.Services;

public class R2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly R2Settings _settings;
    private readonly ILogger<R2FileStorageService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static string SanitizeForLog(string value) =>
        value.Replace("\r", string.Empty).Replace("\n", string.Empty);

    public R2FileStorageService(
        IAmazonS3 s3Client,
        IOptions<FileStorageSettings> settings,
        ILogger<R2FileStorageService> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value.R2;
        _logger = logger;
    }

    public async Task<Result<string>> ReplaceAsync(IFormFile newFile, string directory, string? oldFilePath)
    {
        if (!string.IsNullOrEmpty(oldFilePath))
            await DeleteAsync(oldFilePath);
        
        return await UploadAsync(newFile, directory);
    }

    public async Task<Result<string>> UploadAsync(IFormFile file, string directory)
    {
        if (file is null || file.Length == 0)
            return Result<string>.Failure("No file provided");
        
        if (file.Length > MaxFileSizeBytes)
            return Result<string>.Failure("File exceeds maximum size of 5 MB");
        
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            return Result<string>.Failure($"File type '{extension}' is not allowed");
        
        // Sanitize directory to prevent path traversal
        var safeDirectory = directory.Replace("..", "").Trim('/');
        var key = $"{safeDirectory}/{Guid.NewGuid()}{extension}";

        try
        {
            using var stream = file.OpenReadStream();

            var putRequest = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                DisablePayloadSigning = true // Required for R2
            };

            await _s3Client.PutObjectAsync(putRequest);

            var publicUrl = $"{_settings.PublicUrl.TrimEnd('/')}/{key}";

            _logger.LogInformation(
                "File uploaded to R2: {Key} ({Size} bytes)", key, file.Length);

            return Result<string>.Success(publicUrl);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "R2 upload failed for key {Key}", key);
            return Result<string>.Failure("File upload failed. Please try again.");
        }
    }

    public async Task<Result> DeleteAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return Result.Failure("File path is empty");
        
        try
        {
            var key = ExtractKeyFromUrl(filePath);
            if (key is null)
                return Result.Failure("Invalid file path.");
            
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);

            _logger.LogInformation("File deleted from R2: {Key}", SanitizeForLog(key));

            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "R2 delete failed for path {FilePath}", SanitizeForLog(filePath));
            return Result.Failure("File deletion failed. Please try again.");
        }
    }

    /// <summary>
    /// Extracts the R2 object key from a public URL.
    /// Example: "https://cdn.letopia.com/avatarts/abc.jpg" -> "avatars/abc.jpg"
    /// </summary>
    private static string? ExtractKeyFromUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;
        
        var path = uri.AbsolutePath.TrimStart('/');
        return string.IsNullOrEmpty(path) ? null : path;
    }
}