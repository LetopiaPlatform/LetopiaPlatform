
using LetopiaPlatform.Core.Enums;

namespace LetopiaPlatform.Core.Common;
public class PrivacySettings
{
    public bool ShowPhoneNumber { get; set; }
    public bool ShowEmailAddress { get; set; } 
    public bool ShowProjects { get; set; } = true;
    public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.Public;
}
