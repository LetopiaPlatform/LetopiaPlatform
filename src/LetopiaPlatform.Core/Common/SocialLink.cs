
using System.Text.Json.Serialization;

namespace LetopiaPlatform.Core.Common;



public class SocialLink
{

    public string Provider { get; private set; } = string.Empty;


    public string Url { get; private set; } = string.Empty;


    public SocialLink(string provider, string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("URL must use http or https scheme.", nameof(url));

        Provider = provider;
        Url = url;
    }
}
