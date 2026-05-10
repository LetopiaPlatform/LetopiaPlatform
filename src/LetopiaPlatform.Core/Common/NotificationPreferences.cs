using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetopiaPlatform.Core.Common;
public class NotificationPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool TaskReminders { get; set; } = true;
    public bool AchievementAlerts { get; set; } = true;
    public bool CommunityUpdates { get; set; } = true;
    public bool WeeklyDigest { get; set; } = true;
}
