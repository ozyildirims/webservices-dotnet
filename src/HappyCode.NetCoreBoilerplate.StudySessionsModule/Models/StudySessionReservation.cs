using System.ComponentModel.DataAnnotations;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Models;

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Attended,
    NoShow
}

public class StudySessionReservation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = default!;

    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual StudySession Session { get; set; } = default!;
    public virtual User Student { get; set; } = default!;
} 