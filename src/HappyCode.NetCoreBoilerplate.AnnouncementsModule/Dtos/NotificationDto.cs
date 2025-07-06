namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;

/// <summary>
/// Data transfer object for user notification information
/// </summary>
public class UserNotificationDto
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who received the notification
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The announcement associated with this notification
    /// </summary>
    public AnnouncementDto Announcement { get; set; } = new();

    /// <summary>
    /// Whether the notification has been read
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Date and time when the notification was read
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Date and time when the notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for notification setting information
/// </summary>
public class NotificationSettingDto
{
    /// <summary>
    /// Unique identifier for the setting
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who owns this setting
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of notification (e.g., "Email", "Push", "SMS")
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this notification type is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Date and time when the setting was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the setting was last modified
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Data transfer object for updating notification settings
/// </summary>
public class UpdateNotificationSettingsDto
{
    /// <summary>
    /// Dictionary of notification types and their enabled status
    /// </summary>
    public Dictionary<string, bool> Settings { get; set; } = new();
}

/// <summary>
/// Data transfer object for user device information
/// </summary>
public class UserDeviceDto
{
    /// <summary>
    /// Unique identifier for the device
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who owns this device
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Device token for push notifications
    /// </summary>
    public string DeviceToken { get; set; } = string.Empty;

    /// <summary>
    /// Type of device (e.g., "iOS", "Android", "Web")
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Whether the device is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time when the device was last used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Date and time when the device was registered
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for registering a new device
/// </summary>
public class RegisterDeviceDto
{
    /// <summary>
    /// Device token for push notifications
    /// </summary>
    public string DeviceToken { get; set; } = string.Empty;

    /// <summary>
    /// Type of device (e.g., "iOS", "Android", "Web")
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;
} 