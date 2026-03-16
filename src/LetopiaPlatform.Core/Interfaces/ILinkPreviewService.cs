


using LetopiaPlatform.Core.DTOs.CommunityResourse;

namespace LetopiaPlatform.Core.Interfaces;
/// <summary>
/// Provides functionality to generate metadata previews for external links.
/// The preview typically includes title, description, and a representative image
/// extracted from the target webpage.
/// </summary>
public interface ILinkPreviewService
{
    /// <summary>
    /// Retrieves preview metadata for the specified URL.
    /// </summary>
    /// <param name="url">
    /// The absolute URL of the resource to preview.
    /// </param>
    /// <returns>
    /// A <see cref="LinkPreviewDto"/> containing metadata such as title,
    /// description, image, and the original URL.
    /// </returns>
    /// <remarks>
    /// The service attempts to extract Open Graph metadata
    /// (<c>og:title</c>, <c>og:description</c>, <c>og:image</c>) from the target page.
    /// <para/>
    /// For YouTube links, the thumbnail is resolved directly from the video ID
    /// without scraping the webpage.
    /// </remarks>
    Task<LinkPreviewDto> GetPreviewAsync(string url);
}
