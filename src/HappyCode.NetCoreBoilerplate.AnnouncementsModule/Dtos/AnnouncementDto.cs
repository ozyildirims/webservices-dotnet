namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule.Dtos;

public class AnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<AnnouncementTargetDto> Targets { get; set; } = new();
}

public class AnnouncementTargetDto
{
    public Guid Id { get; set; }
    public string TargetType { get; set; } = default!;
    public Guid TargetId { get; set; }
}

public class CreateAnnouncementDto
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Priority { get; set; }
    public List<AnnouncementTargetDto> Targets { get; set; } = new();
}

public class UpdateAnnouncementDto
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public List<AnnouncementTargetDto> Targets { get; set; } = new();
} 