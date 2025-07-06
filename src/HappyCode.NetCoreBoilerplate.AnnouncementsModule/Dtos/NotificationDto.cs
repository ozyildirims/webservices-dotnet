namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;

public class UserNotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AnnouncementDto Announcement { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationSettingDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class UpdateNotificationSettingsDto
{
    public Dictionary<string, bool> Settings { get; set; } = new();
}

public class UserDeviceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } = default!;
    public string DeviceType { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RegisterDeviceDto
{
    public string DeviceToken { get; set; } = default!;
    public string DeviceType { get; set; } = default!;
} 