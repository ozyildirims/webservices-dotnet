namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;

/// <summary>
/// Data transfer object for updating an existing announcement
/// </summary>
public class UpdateAnnouncementDto
{
    /// <summary>
    /// Title of the announcement
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content/body of the announcement
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Start date when the announcement becomes active
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date when the announcement expires (optional)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Whether the announcement is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Priority level of the announcement (higher number = higher priority)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// List of targets for this announcement
    /// </summary>
    public List<AnnouncementTargetDto> Targets { get; set; } = new();
} 