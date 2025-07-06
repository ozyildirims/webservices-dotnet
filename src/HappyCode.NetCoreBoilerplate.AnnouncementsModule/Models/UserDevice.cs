using System.ComponentModel.DataAnnotations;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;

public class UserDevice
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string DeviceToken { get; set; } = default!;

    [Required]
    [MaxLength(50)]
    public string DeviceType { get; set; } = default!;

    public bool IsActive { get; set; }
    public DateTime LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public virtual User User { get; set; } = default!;
} 