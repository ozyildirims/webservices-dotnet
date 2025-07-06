using System.ComponentModel.DataAnnotations;
using HappyCode.NetCoreBoilerplate.Core.Models;

namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Models;

public class StudySession
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentCapacity { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    public bool IsActive { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual User Teacher { get; set; } = default!;
    public virtual ICollection<StudySessionReservation> Reservations { get; set; } = new List<StudySessionReservation>();
    public virtual ICollection<StudySessionWaitlist> WaitlistEntries { get; set; } = new List<StudySessionWaitlist>();
    public virtual StudySessionRecurrenceRule? RecurrenceRule { get; set; }
} 