
using System.Text.Json.Serialization;

namespace LetopiaPlatform.Core.Common;



public class SocialLink
{

    public string Provider { get; private set; } = string.Empty;


    public string Url { get; private set; } = string.Empty;


    public SocialLink(string provider, string url)
    {
        Provider = provider;
        Url = url;
    }
}
