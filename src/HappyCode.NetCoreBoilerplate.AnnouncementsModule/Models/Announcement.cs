using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;

public class Announcement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    [Required]
    public string Content { get; set; } = default!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual ICollection<AnnouncementTarget> Targets { get; set; } = new List<AnnouncementTarget>();
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
} 