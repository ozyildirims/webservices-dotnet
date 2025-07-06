using System.ComponentModel.DataAnnotations;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;

public class NotificationSetting
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string NotificationType { get; set; } = default!;

    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation property
    public virtual User User { get; set; } = default!;
} 