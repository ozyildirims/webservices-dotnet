using System.ComponentModel.DataAnnotations;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;

public class UserNotification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid AnnouncementId { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = default!;
    public virtual Announcement Announcement { get; set; } = default!;
} 