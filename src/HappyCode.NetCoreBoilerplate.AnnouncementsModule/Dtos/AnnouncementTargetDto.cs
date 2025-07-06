namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;

/// <summary>
/// Data transfer object for announcement target information
/// </summary>
public class AnnouncementTargetDto
{
    /// <summary>
    /// Unique identifier for the target
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of target (e.g., "User", "Role")
    /// </summary>
    public string TargetType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target (user ID or role ID)
    /// </summary>
    public Guid TargetId { get; set; }
} 