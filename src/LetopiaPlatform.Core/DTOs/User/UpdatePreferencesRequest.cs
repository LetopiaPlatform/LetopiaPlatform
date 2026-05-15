using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LetopiaPlatform.Core.Common;

namespace LetopiaPlatform.Core.DTOs.User;
public sealed record UpdatePreferencesRequest(
    NotificationPreferences? NotificationPreferences,

    PrivacySettings? PrivacySettings);
