namespace LetopiaPlatform.Core.AppSettings;

/// <summary>
/// Configuration for file storage provider.
/// Provider: "Local" (development) or "R2" (staging/production).
/// </summary>
public class FileStorageSettings
{
    public const string SectionName = "FileStorage";
    public string Provider {get; set;} = "Local";
    public R2Settings R2 { get; set; } = new();
}

public class R2Settings
{
    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "letopia-uploads";
    public string PublicUrl { get; set; } = string.Empty;   
}