using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Models;

public enum RecurrencePattern
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly
}

public class StudySessionRecurrenceRule
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Pattern { get; set; } = default!;

    [MaxLength(100)]
    public string? DaysOfWeek { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation property
    public virtual StudySession Session { get; set; } = default!;
} 