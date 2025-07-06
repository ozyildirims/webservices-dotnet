using System.ComponentModel.DataAnnotations;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Models;

public enum WaitlistStatus
{
    Active,
    Offered,
    Accepted,
    Declined,
    Expired
}

public class StudySessionWaitlist
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    public int Position { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual StudySession Session { get; set; } = default!;
    public virtual User Student { get; set; } = default!;
} 