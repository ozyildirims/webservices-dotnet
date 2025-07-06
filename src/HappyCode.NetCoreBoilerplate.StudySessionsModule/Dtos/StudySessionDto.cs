namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Dtos;

public class StudySessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentCapacity { get; set; }
    public string? Location { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = default!;
    public Guid SubjectId { get; set; }
    public bool IsActive { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public StudySessionRecurrenceRuleDto? RecurrenceRule { get; set; }
    public List<StudySessionReservationDto> Reservations { get; set; } = new();
}

public class CreateStudySessionDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxCapacity { get; set; }
    public string? Location { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public CreateStudySessionRecurrenceRuleDto? RecurrenceRule { get; set; }
}

public class UpdateStudySessionDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxCapacity { get; set; }
    public string? Location { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsActive { get; set; }
    public UpdateStudySessionRecurrenceRuleDto? RecurrenceRule { get; set; }
}

public class CancelStudySessionDto
{
    public string CancellationReason { get; set; } = default!;
}

public class StudySessionRecurrenceRuleDto
{
    public Guid Id { get; set; }
    public string Pattern { get; set; } = default!;
    public string? DaysOfWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class CreateStudySessionRecurrenceRuleDto
{
    public string Pattern { get; set; } = default!;
    public string? DaysOfWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class UpdateStudySessionRecurrenceRuleDto
{
    public string Pattern { get; set; } = default!;
    public string? DaysOfWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
} 