using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.ExamsModule.Models;

public class ExamResult : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid ExamDefinitionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalScore { get; set; }
    public decimal Percentage { get; set; }
    public ExamStatus Status { get; set; }
    
    public required ExamDefinition ExamDefinition { get; set; }
    public required ICollection<SectionResult> SectionResults { get; set; } = new List<SectionResult>();
}

public class SectionResult : BaseEntity
{
    public Guid ExamResultId { get; set; }
    public Guid ExamSectionId { get; set; }
    public int Score { get; set; }
    public decimal Percentage { get; set; }
    public TimeSpan TimeTaken { get; set; }
    
    public required ExamResult ExamResult { get; set; }
    public required ExamSection ExamSection { get; set; }
}

public enum ExamStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Abandoned = 3
} 