using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;

public class AnnouncementTarget
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AnnouncementId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TargetType { get; set; } = default!;

    [Required]
    public Guid TargetId { get; set; }

    // Navigation property
    public virtual Announcement Announcement { get; set; } = default!;
} 