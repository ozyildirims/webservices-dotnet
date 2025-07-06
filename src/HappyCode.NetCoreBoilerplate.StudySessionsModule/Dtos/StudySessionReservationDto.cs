namespace HappyCode.NetCoreBoilerplate.StudySessionsModule.Dtos;

public class StudySessionReservationDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class CreateStudySessionReservationDto
{
    public Guid SessionId { get; set; }
}

public class CancelStudySessionReservationDto
{
    public string CancellationReason { get; set; } = default!;
}

public class StudySessionWaitlistDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;
    public int Position { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class JoinWaitlistDto
{
    public Guid SessionId { get; set; }
}

public class WaitlistOfferResponseDto
{
    public bool Accept { get; set; }
} 