namespace LetopiaPlatform.Core.Enums;
/// <summary>
/// Defines the current state of a project, guiding user expectations and platform behavior.
/// </summary>
public enum ProjectStatus
{

    Available = 1,      // Open for members to join and contribute
    UnAvailable = 2,    // Not open for new members, but work is ongoing
    Recruiting = 3,
    InProgress = 4,
    Completed = 5,
}
