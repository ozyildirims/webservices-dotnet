using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Models;

public class ExamDefinition : BaseEntity
{
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }

    public TimeSpan Duration { get; set; }
    public int TotalPoints { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ExamType Type { get; set; }
    public Guid CreatedById { get; set; }
    public bool IsPublished { get; set; }
    
    public ICollection<ExamSection> Sections { get; set; } = new List<ExamSection>();
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}

public enum ExamType
{
    Quiz = 0,
    Midterm = 1,
    Final = 2,
    MockExam = 3
} 