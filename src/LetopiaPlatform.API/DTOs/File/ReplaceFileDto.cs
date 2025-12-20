namespace LetopiaPlatform.API.DTOs.File
{
    public class ReplaceFileDto
    {
        public IFormFile NewFile { get; set; } = default!;
        public string Directory { get; set; } = "uploads";
        public string OldFileUrl { get; set; } = string.Empty;
    }
}
